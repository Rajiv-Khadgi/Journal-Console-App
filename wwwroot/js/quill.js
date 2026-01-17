
var quill = null;

function initQuill() {
    const editor = document.querySelector('#editor');
    if (!editor) {
        console.error("Editor not found");
        return;
    }

    quill = new Quill(editor, {
        theme: 'snow'
    });
}

function initQuillWithContent(content) {
    const editor = document.querySelector('#editor');
    if (!editor) {
        console.error("Editor not found");
        return;
    }

    quill = new Quill(editor, {
        theme: 'snow'
    });

    if (content) {
        quill.root.innerHTML = content;
    }
}

function getQuillContent() {
    if (!quill) return "";
    return quill.root.innerHTML;
}

window.initQuill = initQuill;
window.initQuillWithContent = initQuillWithContent;
window.getQuillContent = getQuillContent;