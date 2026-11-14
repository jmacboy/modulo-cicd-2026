import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

// jsdom does not implement the modal API of <dialog>. This minimal polyfill mirrors the browser
// contract the app relies on: `showModal()` opens the dialog and `close()` closes it and fires a
// `close` event (only when it was open).
if (typeof HTMLDialogElement !== 'undefined' && !HTMLDialogElement.prototype.showModal) {
    HTMLDialogElement.prototype.showModal = function showModal() {
        this.setAttribute('open', '');
    };
    HTMLDialogElement.prototype.close = function close() {
        if (!this.hasAttribute('open')) {
            return;
        }
        this.removeAttribute('open');
        this.dispatchEvent(new Event('close'));
    };
}

afterEach(() => {
    cleanup();
});
