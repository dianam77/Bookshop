document.addEventListener('DOMContentLoaded', () => {
    const myNavBtn = document.getElementById('mynavbtn');
    const desktopNav = document.getElementById('desktopnav');

    myNavBtn.addEventListener('click', function () {
        desktopNav.classList.toggle('active');  // Toggle the 'active' class to show/hide the menu
    });
});


// AOS initialization script
AOS.init({
    offset: 120, // Start animation 120px before element enters the viewport
    duration: 1000, // Animation duration in ms
    easing: 'ease', // Easing function
    delay: 100, // Delay before animation starts
    once: true // Trigger animation only once
});

