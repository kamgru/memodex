"use strict";
class CreateProfile {
    init() {
        const selectedAvatarContainer = document.getElementById('selected-avatar-container');
        const availableAvatarsContainer = document.getElementById('available-avatars-container');
        if (!selectedAvatarContainer || !availableAvatarsContainer) {
            showNotification('Error', 'There was an error. The #selected-avatar-container was not found.');
            return;
        }
        selectedAvatarContainer.addEventListener('click', () => {
            availableAvatarsContainer.classList.toggle('hidden');
        });
        availableAvatarsContainer.addEventListener('change', (event) => {
            var _a, _b;
            const input = event.target;
            if (!(input === null || input === void 0 ? void 0 : input.checked)) {
                return;
            }
            (_a = document.getElementById('default-avatar-placeholder')) === null || _a === void 0 ? void 0 : _a.remove();
            availableAvatarsContainer.classList.add('hidden');
            const selectedImage = (_b = input.nextElementSibling) === null || _b === void 0 ? void 0 : _b.src;
            const imageElement = selectedAvatarContainer.querySelector('img');
            imageElement.src = selectedImage;
            selectedAvatarContainer.classList.remove('hidden');
        });
    }
}
new CreateProfile().init();
