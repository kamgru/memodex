"use strict";

class DeckImporter {
    constructor() {
        this.handleFileInputChanged = (event) => {
            var _a;
            const file = (_a = event.target.files) === null || _a === void 0 ? void 0 : _a[0];
            if (!file) {
                return;
            }
            const reader = new FileReader();
            reader.addEventListener('load', this.handleOnFileRead);
            reader.readAsText(file);
        };
        this.handleOnFileRead = (event) => {
            const content = event.target.result;
            try {
                const deck = JSON.parse(content);
                if (!this.isDeck(deck)) {
                    showNotification('Error', 'The selected  JSON file does not contain a valid deck.');
                }
                const form = document.querySelector('#form-import-deck');
                if (!form) {
                    return;
                }
                htmx.trigger(form, 'submit');
            } catch (e) {
                showNotification('Error', 'Invalid file format.');
            }
        };
    }

    init() {
        const fileInput = document.querySelector('#input-import-deck');
        if (fileInput == null) {
            throw new Error('No file upload input found.');
        }
        fileInput.addEventListener('change', this.handleFileInputChanged);
    }

    isDeck(object) {
        return (object !== null &&
            typeof object === 'object' &&
            'name' in object && typeof object.name === 'string' &&
            'description' in object && typeof object.description === 'string' &&
            'flashcards' in object && Array.isArray(object.flashcards) &&
            object.flashcards.every((card) => 'question' in card && typeof card.question === 'string' &&
                'answer' in card && typeof card.answer === 'string'));
    }
}

const deckImporter = new DeckImporter();
deckImporter.init();
