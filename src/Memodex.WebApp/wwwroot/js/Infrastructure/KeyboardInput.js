"use strict";
const contextName = document.location.pathname.split('/')[1]
    .toLowerCase();
if (contextName === 'engage') {
    const form = document.getElementById('engage-controls-form');
    if (form == null) {
        throw new Error('Could not find engage-controls-form');
    }
    const flashcardEngage = document.querySelector('flashcard-engage');
    if (flashcardEngage == null) {
        throw new Error('Could not find FlashcardEngage');
    }
    const markReviewCheckbox = document.getElementById('engage-controls-mark-checkbox');
    if (markReviewCheckbox == null) {
        throw new Error('Could not find engage-controls-mark-checkbox');
    }
    document.addEventListener('keydown', (event) => {
        if (event.key === 'n') {
            form.submit();
        }
        if (event.key === 'm') {
            markReviewCheckbox.checked = !markReviewCheckbox.checked;
        }
        if (event.key === 'r') {
            flashcardEngage.toggleAnswer();
        }
    });
}
else if (contextName === '') {
    document.addEventListener('keydown', (event) => {
        if (event.key == 'd') {
            window.location.assign('/BrowseDecks');
        }
    });
}
