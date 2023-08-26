class CreateProfile {
    public init() {

        const selectedAvatarContainer = document.getElementById('selected-avatar-container') as HTMLDivElement;
        const availableAvatarsContainer = document.getElementById('available-avatars-container') as HTMLDivElement;

        if (!selectedAvatarContainer || !availableAvatarsContainer) {
            showNotification('Error', 'There was an error. The #selected-avatar-container was not found.');
            return;
        }

        selectedAvatarContainer.addEventListener('click', () => {
            availableAvatarsContainer.classList.toggle('hidden');
        });

        availableAvatarsContainer.addEventListener('change', (event: Event) => {
            const input = event.target as HTMLInputElement;

            if (!input?.checked) {
                return;
            }

            document.getElementById('default-avatar-placeholder')?.remove();
            availableAvatarsContainer.classList.add('hidden');
            const selectedImage= (input.nextElementSibling as HTMLImageElement)?.src;
            const imageElement = selectedAvatarContainer.querySelector('img') as HTMLImageElement;
            imageElement.src = selectedImage;
            selectedAvatarContainer.classList.remove('hidden'); 
        });
    }
}

new CreateProfile().init();