"use strict";
class EditCategory {
    init() {
        var _a;
        (_a = document.getElementById('UploadedImage')) === null || _a === void 0 ? void 0 : _a.addEventListener('change', (event) => {
            var _a, _b;
            const file = (_b = (_a = event.target) === null || _a === void 0 ? void 0 : _a.files) === null || _b === void 0 ? void 0 : _b[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    var _a;
                    const image = document.querySelector('#current-image');
                    image.src = (_a = e.target) === null || _a === void 0 ? void 0 : _a.result;
                };
                reader.readAsDataURL(file);
            }
        });
    }
}
new EditCategory().init();
