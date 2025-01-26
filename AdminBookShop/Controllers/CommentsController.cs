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
        public async Task<IActionResult> Index()
        {
            var comments = await _commentRepository.GetAllComments();
            return View(comments);
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Load replies using Lazy Loading
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
                UserId = "Admin", // or retrieve the admin user ID
                UserName = "Admin", // or retrieve the admin user name
                Created = DateTime.Now,
                ReplyId = commentId
            };

            await _commentRepository.Add(reply);
            return RedirectToAction(nameof(Details), new { id = commentId });
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null || comment.UserId != "Admin") // Ensure only admin can edit
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
                await _commentRepository.Update(comment);
                return RedirectToAction(nameof(Details), new { id = comment.ReplyId ?? comment.Id });
            }
            return View(comment);
        }

        // GET: Comments/Delete/5
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
        public async Task<IActionResult> EditReply(int id)
        {
            var reply = await _commentRepository.GetCommentById(id);
            if (reply == null || reply.UserId != "Admin") // Ensure only admin can edit
            {
                return NotFound();
            }
            return View(reply);
        }

        // POST: Comments/EditReply/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReply(int id, Comment reply)
        {
            if (id != reply.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _commentRepository.Update(reply);
                    return RedirectToAction(nameof(Details), new { id = reply.ReplyId ?? reply.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CommentExists(reply.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(reply);
        }

        // GET: Comments/DeleteReply/5
        public async Task<IActionResult> DeleteReply(int id)
        {
            var reply = await _commentRepository.GetCommentById(id);
            if (reply == null || reply.UserId != "Admin") // Ensure only admin can delete
            {
                return NotFound();
            }
            return View(reply);
        }

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
            Console.WriteLine("DeleteReplyConfirmed action called with ID: " + id);
            return RedirectToAction(nameof(Details), new { id = reply.ReplyId ?? reply.BookId });
        }

        private async Task<bool> CommentExists(int id)
        {
            return await _commentRepository.GetCommentById(id) != null;
        }
    }
}
