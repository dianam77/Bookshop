

var navbtn = document.getElementById("mynavbtn");
var desktopnav = document.getElementById("desktopnav");
console.log(navbtn)
navbtn.addEventListener("click", () => {

    if (desktopnav.style.display == "flex") {
        desktopnav.style.display = "none";
    } else {
        desktopnav.style.display = "flex";
        desktopnav.style.flexDirection = "column";
    }

})




let countInput = document.getElementById("countInput");
countInput.addEventListener("change", () => {

    let bookIntPrice = document.getElementById("bookIntPrice");
    let bookShowPrice = document.getElementById("bookShowPrice");
    let count = countInput.value;
    let totalPrice = count * bookIntPrice.value;
    bookShowPrice.innerHTML = `قیمت نهایی : ${totalPrice.toLocaleString()} تومان`;

});

