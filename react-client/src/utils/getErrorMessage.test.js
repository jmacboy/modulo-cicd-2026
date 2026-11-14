import { describe, expect, it } from 'vitest';
import { getErrorMessage } from './getErrorMessage';

const FALLBACK = 'Something went wrong';

const apiError = (status, data) => ({ response: { status, data } });

describe('getErrorMessage', () => {
    it.each([
        ['ItemNotFound', 'The item no longer exists. Refresh the list and try again.'],
        ['NameIsRequired', 'Enter a name for the item.'],
        ['CannotDeleteWithReservedStock', 'The item has reserved stock and cannot be deleted.'],
        ['CreatorNotFound', 'The user ID does not exist. Check it and try again.'],
        ['InsuficentStockToReserve', 'There is not enough available stock.'],
    ])('maps the API error code %s to its client message', (code, expected) => {
        expect(getErrorMessage(apiError(400, { code }), FALLBACK)).toBe(expected);
    });

    it('maps a 404 response without a known code to the item-not-found message', () => {
        expect(getErrorMessage(apiError(404, 'Item not found'), FALLBACK)).toBe(
            'The item no longer exists. Refresh the list and try again.',
        );
    });

    it('prefers the error code over the HTTP status', () => {
        expect(getErrorMessage(apiError(404, { code: 'NameIsRequired' }), FALLBACK)).toBe(
            'Enter a name for the item.',
        );
    });

    it('never shows the backend description for an unknown code', () => {
        const error = apiError(400, { code: 'SomethingNew', description: 'Internal detail' });
        expect(getErrorMessage(error, FALLBACK)).toBe(FALLBACK);
    });

    it('does not resolve inherited object members as codes', () => {
        expect(getErrorMessage(apiError(400, { code: 'constructor' }), FALLBACK)).toBe(FALLBACK);
        expect(getErrorMessage(apiError(400, { code: 'toString' }), FALLBACK)).toBe(FALLBACK);
    });

    it.each([
        ['a network error without response', new Error('Network Error')],
        ['an undefined error', undefined],
        ['a null error', null],
        ['a 500 response', apiError(500, {})],
    ])('falls back to the action message for %s', (_label, error) => {
        expect(getErrorMessage(error, FALLBACK)).toBe(FALLBACK);
    });
});
