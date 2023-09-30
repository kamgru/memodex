const contextName: string = document.location.pathname.split('/')[1]
    .toLowerCase();

if (contextName === 'engage') {

    const form = document.getElementById('engage-controls-form') as HTMLFormElement;
    if (form == null) {
        throw new Error('Could not find engage-controls-form');
    }

    const flashcardEngage = document.querySelector('flashcard-engage') as FlashcardEngage;
    if (flashcardEngage == null) {
        throw new Error('Could not find FlashcardEngage');
    }

    const markReviewCheckbox = document.getElementById('engage-controls-mark-checkbox') as HTMLInputElement;
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
