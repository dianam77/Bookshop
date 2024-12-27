document.addEventListener('DOMContentLoaded', () => {
    const myNavBtn = document.getElementById('mynavbtn');
    const desktopNav = document.getElementById('desktopnav');

    myNavBtn.addEventListener('click', function () {
        desktopNav.classList.toggle('active');  // Toggle the 'active' class to show/hide the menu
    });
});