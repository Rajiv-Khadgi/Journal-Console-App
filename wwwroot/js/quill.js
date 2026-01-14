// quill.js
var quill;

function initQuill() {
    quill = new Quill('#editor', {
        theme: 'snow'
    });
}

function getQuillContent() {
    return quill.root.innerHTML;
}
