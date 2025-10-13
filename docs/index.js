

function startResize(e) {
    e.preventDefault(); // prevent text selection
    console.log("tried");
    const box = e.target.parentElement; // the div being resized
    const startX = e.clientX;
    const startWidth = box.offsetWidth * (100 / [document.documentElement.clientWidth]);


    function doResize(e) {
        box.style.width = (startWidth + (e.clientX - startX) * (100 / [document.documentElement.clientWidth])) + "vw";
    }

    function stopResize() {
        window.removeEventListener('mousemove', doResize);
        window.removeEventListener('mouseup', stopResize);
    }

    window.addEventListener('mousemove', doResize);
    window.addEventListener('mouseup', stopResize);
}

const handles = document.querySelectorAll('.handle');
handles.forEach(handle => {
    handle.addEventListener('mousedown', startResize);
});



