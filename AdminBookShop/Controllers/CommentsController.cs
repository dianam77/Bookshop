using DataAccess.Models;
using DataAccess.Repositories.CommentRepo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminBookShop.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentRepository _commentRepository;

        public CommentsController(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        // GET: Comments
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comments = await _commentRepository.GetAllComments();
            return View(comments);
        }

        // GET: Comments/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Load replies
            var replies = await _commentRepository.GetRepliesByCommentId(id);
            comment.Replies = replies.ToList();

            return View(comment);
        }

        // POST: Comments/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int commentId, string replyText)
        {
            var comment = await _commentRepository.GetCommentById(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var reply = new Comment
            {
                Text = replyText,
                BookId = comment.BookId,
                UserId = "Admin",
                UserName = "Admin",
                Created = DateTime.Now,
                ReplyId = commentId
            };

            await _commentRepository.Add(reply);
            return RedirectToAction(nameof(Details), new { id = commentId });
        }

        // GET: Comments/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _commentRepository.Update(comment);
                    return RedirectToAction(nameof(Details), new { id = comment.ReplyId ?? comment.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(comment);
        }

        // GET: Comments/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
            {
                return NotFound();
            }

            await _commentRepository.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Comments/EditReply/5
        [HttpGet]
        public async Task<IActionResult> EditReply(int id)
        {
            var reply = await _commentRepository.GetCommentById(id);
            if (reply == null || reply.UserId != "Admin")
            {
                return NotFound();
            }
            return View(reply);
        }

        // POST: Comments/EditReply/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int id, Comment updatedComment)
        {
            if (id != updatedComment.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _commentRepository.Update(updatedComment);
                    return RedirectToAction(nameof(Details), new { id = updatedComment.ReplyId ?? updatedComment.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CommentExists(updatedComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(updatedComment);
        }

        // GET: Comments/DeleteReply/5
        [HttpGet]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _commentRepository.GetCommentById(id);
            if (reply == null || reply.UserId != "Admin")
            {
                return NotFound();
            }
            return View(reply);
        }

        // POST: Comments/DeleteReply/5
        [HttpPost, ActionName("DeleteReply")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReplyConfirmed(int id)
        {
            var reply = await _commentRepository.GetCommentById(id);
            if (reply == null)
            {
                return NotFound();
            }

            await _commentRepository.Delete(id);
            return RedirectToAction(nameof(Details), new { id = reply.ReplyId ?? reply.BookId });
        }

        private async Task<bool> CommentExists(int id)
        {
            return await _commentRepository.GetCommentById(id) != null;
        }
    }
}
