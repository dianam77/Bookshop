
var navbtn = document.getElementById("mynavbtn");
var desktopnav = document.getElementById("desktopnav");

navbtn.addEventListener("click", () => {

    if (desktopnav.style.display == "flex") {
        desktopnav.style.display = "none";
    } else {
        desktopnav.style.display = "flex";
        desktopnav.style.flexDirection = "column";
    }

})
