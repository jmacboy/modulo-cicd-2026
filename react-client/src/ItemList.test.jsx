import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import ItemList from './ItemList';
import { ItemService } from './services/ItemService';

vi.mock('./services/ItemService', () => ({ ItemService: vi.fn() }));

const PEN = { id: 'item-1', itemName: 'Pen', stock: 10, available: 8, unitaryCost: 1.5 };
const BOOK = { id: 'item-2', itemName: 'Book', stock: 2, available: 2, unitaryCost: 12 };

const rowOf = (text) => screen.getByRole('cell', { name: text }).closest('tr');

describe('ItemList', () => {
    let service;

    beforeEach(() => {
        service = {
            getItemList: vi.fn().mockResolvedValue([PEN, BOOK]),
            deleteItem: vi.fn().mockResolvedValue({}),
            updateItem: vi.fn().mockResolvedValue({}),
            addStock: vi.fn().mockResolvedValue({}),
        };
        ItemService.mockReturnValue(service);
    });

    it('renders every loaded item with its cost formatted to two decimals', async () => {
        render(<ItemList reload={1} />);

        const penRow = await screen.findByRole('cell', { name: 'Pen' });
        const cells = within(penRow.closest('tr')).getAllByRole('cell');
        expect(cells.map((cell) => cell.textContent)).toEqual(
            expect.arrayContaining(['item-1', 'Pen', '10', '8', '1.50']),
        );
        expect(within(rowOf('Book')).getByText('12.00')).toBeInTheDocument();
    });

    it('renders an empty table when the API returns no items', async () => {
        service.getItemList.mockResolvedValue(null);

        render(<ItemList reload={1} />);

        await waitFor(() => expect(service.getItemList).toHaveBeenCalled());
        expect(screen.queryAllByRole('row')).toHaveLength(1);
    });

    it('shows an alert when the list cannot be loaded', async () => {
        service.getItemList.mockRejectedValue(new Error('Network Error'));

        render(<ItemList reload={1} />);

        expect(await screen.findByRole('alert')).toHaveTextContent(
            'The item list could not be loaded',
        );
    });

    it('reloads the list when the reload prop changes', async () => {
        const { rerender } = render(<ItemList reload={1} />);
        await screen.findByRole('cell', { name: 'Pen' });

        rerender(<ItemList reload={2} />);

        await waitFor(() => expect(service.getItemList).toHaveBeenCalledTimes(2));
    });

    describe('delete', () => {
        it('removes the item from the table after the API deletes it', async () => {
            const user = userEvent.setup();
            render(<ItemList reload={1} />);
            await screen.findByRole('cell', { name: 'Pen' });

            await user.click(within(rowOf('Pen')).getByRole('button', { name: 'Delete' }));

            await waitFor(() =>
                expect(screen.queryByRole('cell', { name: 'Pen' })).not.toBeInTheDocument(),
            );
            expect(service.deleteItem).toHaveBeenCalledWith('item-1');
            expect(screen.getByRole('cell', { name: 'Book' })).toBeInTheDocument();
        });

        it('keeps the item and shows the mapped message when deletion fails', async () => {
            service.deleteItem.mockRejectedValue({
                response: { status: 400, data: { code: 'CannotDeleteWithReservedStock' } },
            });
            const user = userEvent.setup();
            render(<ItemList reload={1} />);
            await screen.findByRole('cell', { name: 'Pen' });

            await user.click(within(rowOf('Pen')).getByRole('button', { name: 'Delete' }));

            expect(await screen.findByRole('alert')).toHaveTextContent(
                'The item has reserved stock and cannot be deleted.',
            );
            expect(screen.getByRole('cell', { name: 'Pen' })).toBeInTheDocument();
        });
    });

    describe('edit', () => {
        const startEditingPen = async (user) => {
            render(<ItemList reload={1} />);
            await screen.findByRole('cell', { name: 'Pen' });
            await user.click(within(rowOf('Pen')).getByRole('button', { name: 'Edit' }));
            return screen.getByRole('textbox', { name: 'Item name' });
        };

        it('shows the current name in a focused input', async () => {
            const user = userEvent.setup();

            const input = await startEditingPen(user);

            expect(input).toHaveValue('Pen');
            expect(input).toHaveFocus();
        });

        it('saves the new name with the Save button', async () => {
            const user = userEvent.setup();
            const input = await startEditingPen(user);

            await user.clear(input);
            await user.type(input, 'Pencil');
            await user.click(screen.getByRole('button', { name: 'Save' }));

            expect(await screen.findByRole('cell', { name: 'Pencil' })).toBeInTheDocument();
            expect(service.updateItem).toHaveBeenCalledWith('item-1', { name: 'Pencil' });
            expect(screen.queryByRole('textbox', { name: 'Item name' })).not.toBeInTheDocument();
        });

        it('saves the new name when Enter is pressed', async () => {
            const user = userEvent.setup();
            const input = await startEditingPen(user);

            await user.clear(input);
            await user.type(input, 'Marker{Enter}');

            expect(await screen.findByRole('cell', { name: 'Marker' })).toBeInTheDocument();
            expect(service.updateItem).toHaveBeenCalledWith('item-1', { name: 'Marker' });
        });

        it('cancels the edit with Escape without calling the API', async () => {
            const user = userEvent.setup();
            const input = await startEditingPen(user);

            await user.type(input, 'X{Escape}');

            expect(screen.getByRole('cell', { name: 'Pen' })).toBeInTheDocument();
            expect(service.updateItem).not.toHaveBeenCalled();
        });

        it('cancels the edit with the Cancel button', async () => {
            const user = userEvent.setup();
            await startEditingPen(user);

            await user.click(screen.getByRole('button', { name: 'Cancel' }));

            expect(screen.queryByRole('textbox', { name: 'Item name' })).not.toBeInTheDocument();
            expect(screen.getByRole('cell', { name: 'Pen' })).toBeInTheDocument();
        });

        it('does not save a blank name', async () => {
            const user = userEvent.setup();
            const input = await startEditingPen(user);

            await user.clear(input);
            await user.type(input, '   {Enter}');

            expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
            expect(service.updateItem).not.toHaveBeenCalled();
        });

        it('keeps editing and shows the mapped message when the update fails', async () => {
            service.updateItem.mockRejectedValue({ response: { status: 404, data: 'Not found' } });
            const user = userEvent.setup();
            const input = await startEditingPen(user);

            await user.type(input, 's{Enter}');

            expect(await screen.findByRole('alert')).toHaveTextContent(
                'The item no longer exists. Refresh the list and try again.',
            );
            expect(screen.getByRole('textbox', { name: 'Item name' })).toHaveValue('Pens');
        });
    });

    describe('add stock', () => {
        it('opens the dialog for the selected item', async () => {
            const user = userEvent.setup();
            render(<ItemList reload={1} />);
            await screen.findByRole('cell', { name: 'Book' });

            await user.click(within(rowOf('Book')).getByRole('button', { name: 'Add stock' }));

            expect(screen.getByRole('dialog')).toHaveAttribute('open');
            expect(screen.getByRole('heading', { name: 'Add stock: Book' })).toBeInTheDocument();
        });

        it('reloads the list after stock is added', async () => {
            const user = userEvent.setup();
            render(<ItemList reload={1} />);
            await screen.findByRole('cell', { name: 'Book' });
            await user.click(within(rowOf('Book')).getByRole('button', { name: 'Add stock' }));

            const dialog = screen.getByRole('dialog');
            await user.type(within(dialog).getByLabelText('Quantity'), '3');
            await user.type(within(dialog).getByLabelText('Unitary cost'), '4');
            await user.type(within(dialog).getByLabelText('User GUID'), 'user-1');
            await user.click(within(dialog).getByRole('button', { name: 'Add stock' }));

            await waitFor(() => expect(service.getItemList).toHaveBeenCalledTimes(2));
            expect(service.addStock).toHaveBeenCalledWith('item-2', {
                userCreatorId: 'user-1',
                quantity: 3,
                unitaryCost: 4,
            });
            expect(screen.getByRole('dialog', { hidden: true })).not.toHaveAttribute('open');
        });
    });
});
