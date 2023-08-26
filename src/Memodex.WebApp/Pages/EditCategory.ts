class EditCategory {
    public init() {

        document.getElementById('UploadedImage')?.addEventListener('change', (event: Event) => {
            const file = (event.target as HTMLInputElement)?.files?.[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const image = document.querySelector('#current-image') as HTMLImageElement;
                    image.src = (e.target as FileReader)?.result as string;
                }
                reader.readAsDataURL(file);
            }
        });
    }
}

new EditCategory().init();