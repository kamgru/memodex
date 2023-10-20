let contextName: string | undefined;

const contextElement: HTMLElement | null = document.querySelector('[data-input-context]');

if (contextElement !== null) {
    contextName = contextElement.dataset.inputContext;
}

if (contextName === 'engage') {

    const form = document.getElementById('engage-controls-form') as HTMLFormElement;
    if (!form) {
        throw new Error('Could not find engage-controls-form');
    }

    const flashcardEngage = document.querySelector('flashcard-engage') as FlashcardEngage;
    if (!flashcardEngage) {
        throw new Error('Could not find FlashcardEngage');
    }

    const markReviewCheckbox = document.getElementById('engage-controls-mark-checkbox') as HTMLInputElement;
    if (!markReviewCheckbox) {
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
} else if (contextName === 'review') {
    
    const form = document.getElementById('review-controls-form') as HTMLFormElement;
    if (!form) {
        throw new Error('Could not find review-controls-form');
    }

    const flashcardEngage = document.querySelector('flashcard-engage') as FlashcardEngage;
    if (!flashcardEngage) {
        throw new Error('Could not find FlashcardEngage');
    }

    document.addEventListener('keydown', (event) => {
        if (event.key === 'n') {
            form.submit();
        }
        if (event.key === 'r') {
            flashcardEngage.toggleAnswer();
        }
    });
}
