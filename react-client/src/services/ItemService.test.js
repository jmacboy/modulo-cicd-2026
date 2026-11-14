import axios from 'axios';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ItemService } from './ItemService';

vi.mock('axios', () => ({
    default: {
        get: vi.fn(),
        post: vi.fn(),
        put: vi.fn(),
        delete: vi.fn(),
    },
}));

const BASE_URL = 'http://api.test';
const JSON_HEADERS = { headers: { 'Content-Type': 'application/json' } };

describe('ItemService', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.spyOn(console, 'log').mockImplementation(() => {});
    });

    it('uses the local API as the default base URL', async () => {
        axios.delete.mockResolvedValue({ data: {} });

        await ItemService().deleteItem('abc');

        expect(axios.delete).toHaveBeenCalledWith('https://localhost:7297/api/Item/abc');
    });

    it('uses VITE_API_URL as the default base URL when it is set', async () => {
        vi.stubEnv('VITE_API_URL', 'http://api.from.env');
        axios.delete.mockResolvedValue({ data: {} });

        await ItemService().deleteItem('abc');

        expect(axios.delete).toHaveBeenCalledWith('http://api.from.env/api/Item/abc');
        vi.unstubAllEnvs();
    });

    describe('getItemList', () => {
        it('resolves with the items inside the result value', async () => {
            const items = [{ id: '1', itemName: 'Pen' }];
            axios.get.mockResolvedValue({ data: { value: items, isSuccess: true } });

            await expect(ItemService(BASE_URL).getItemList()).resolves.toEqual(items);
            expect(axios.get).toHaveBeenCalledWith(`${BASE_URL}/api/Item`, JSON_HEADERS);
        });

        it('rejects with the axios error', async () => {
            const error = new Error('Network Error');
            axios.get.mockRejectedValue(error);

            await expect(ItemService(BASE_URL).getItemList()).rejects.toBe(error);
        });
    });

    describe('addItem', () => {
        it('posts the item and resolves with the response body', async () => {
            const item = { id: 'guid-1', itemName: 'Pen' };
            axios.post.mockResolvedValue({ data: 'guid-1' });

            await expect(ItemService(BASE_URL).addItem(item)).resolves.toBe('guid-1');
            expect(axios.post).toHaveBeenCalledWith(`${BASE_URL}/api/Item`, item, JSON_HEADERS);
        });

        it('rejects with the axios error', async () => {
            const error = new Error('Bad Request');
            axios.post.mockRejectedValue(error);

            await expect(ItemService(BASE_URL).addItem({})).rejects.toBe(error);
        });
    });

    describe('addStock', () => {
        it('posts only the stock fields to the item stock endpoint', async () => {
            axios.post.mockResolvedValue({ data: { isSuccess: true } });

            const result = await ItemService(BASE_URL).addStock('item-1', {
                userCreatorId: 'user-1',
                quantity: 5,
                unitaryCost: 2.5,
                extra: 'ignored',
            });

            expect(result).toEqual({ isSuccess: true });
            expect(axios.post).toHaveBeenCalledWith(
                `${BASE_URL}/api/Item/item-1/stock`,
                { userCreatorId: 'user-1', quantity: 5, unitaryCost: 2.5 },
                JSON_HEADERS,
            );
        });

        it('rejects with the axios error', async () => {
            const error = new Error('Bad Request');
            axios.post.mockRejectedValue(error);

            await expect(
                ItemService(BASE_URL).addStock('item-1', {
                    userCreatorId: 'u',
                    quantity: 1,
                    unitaryCost: 1,
                }),
            ).rejects.toBe(error);
        });
    });

    describe('deleteItem', () => {
        it('deletes the item and resolves with the response body', async () => {
            axios.delete.mockResolvedValue({ data: { isSuccess: true } });

            await expect(ItemService(BASE_URL).deleteItem('item-1')).resolves.toEqual({
                isSuccess: true,
            });
            expect(axios.delete).toHaveBeenCalledWith(`${BASE_URL}/api/Item/item-1`);
        });

        it('rejects with the axios error', async () => {
            const error = new Error('Not Found');
            axios.delete.mockRejectedValue(error);

            await expect(ItemService(BASE_URL).deleteItem('item-1')).rejects.toBe(error);
        });
    });

    describe('updateItem', () => {
        it('puts the new data and resolves with the response body', async () => {
            axios.put.mockResolvedValue({ data: { isSuccess: true } });

            await expect(
                ItemService(BASE_URL).updateItem('item-1', { name: 'Pencil' }),
            ).resolves.toEqual({ isSuccess: true });
            expect(axios.put).toHaveBeenCalledWith(
                `${BASE_URL}/api/Item/item-1`,
                { name: 'Pencil' },
                JSON_HEADERS,
            );
        });

        it('rejects with the axios error', async () => {
            const error = new Error('Not Found');
            axios.put.mockRejectedValue(error);

            await expect(ItemService(BASE_URL).updateItem('item-1', {})).rejects.toBe(error);
        });
    });
});
