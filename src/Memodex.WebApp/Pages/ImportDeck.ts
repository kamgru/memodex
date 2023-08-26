declare var htmx: any;

interface IFlashcard {
    question: string
    answer: string
}

interface IDeck {
    name: string
    description: string
    flashcards: IFlashcard[]
}

class DeckImporter {

    public init() {
        const fileInput = document.querySelector('#input-import-deck') as HTMLInputElement;

        if (fileInput == null) {
            throw new Error('No file upload input found.');
        }

        fileInput.addEventListener('change', this.handleFileInputChanged);
    }

    private handleFileInputChanged = (event: Event): void => {
        const earlierConfirmation = document.querySelector('#deck-import-confirmation') as HTMLInputElement;

        if (earlierConfirmation) {
        }
        
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) {
            return;
        }
        const reader = new FileReader();
        reader.addEventListener('load', this.handleOnFileRead);
        reader.readAsText(file);
    }

    private handleOnFileRead = (event: ProgressEvent<FileReader>): void => {
        const content = (event.target as FileReader).result as string;

        try {
            const deck = JSON.parse(content) as IDeck;
            if (!this.isDeck(deck)) {
                showNotification('Error', 'The selected  JSON file does not contain a valid deck.');
            }
            const form = document.querySelector('#form-import-deck') as HTMLFormElement;
            if (!form){
                return;
            }
            htmx.trigger(form, 'submit');

        } catch (e) {
            showNotification('Error', 'Invalid file format.');
        }
    }

    private isDeck(object: any): object is IDeck {
        return (
            object !== null &&
            typeof object === 'object' &&
            'name' in object && typeof object.name === 'string' &&
            'description' in object && typeof object.description === 'string' &&
            'flashcards' in object && Array.isArray(object.flashcards) &&
            object.flashcards.every((card: any) =>
                'question' in card && typeof card.question === 'string' &&
                'answer' in card && typeof card.answer === 'string'
            )
        );
    }

}

const deckImporter = new DeckImporter();
deckImporter.init();