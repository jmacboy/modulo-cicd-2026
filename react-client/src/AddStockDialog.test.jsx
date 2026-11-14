import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import AddStockDialog from './AddStockDialog';
import { ItemService } from './services/ItemService';

vi.mock('./services/ItemService', () => ({ ItemService: vi.fn() }));

const ITEM = { id: 'item-1', itemName: 'Pen', stock: 3, available: 3, unitaryCost: 1.5 };

const deferred = () => {
    let resolve;
    let reject;
    const promise = new Promise((res, rej) => {
        resolve = res;
        reject = rej;
    });
    return { promise, resolve, reject };
};

const getDialog = () => screen.getByRole('dialog', { hidden: true });

const fillForm = async (user, { quantity = '5', cost = '2.5', userId = 'user-1' } = {}) => {
    await user.type(screen.getByLabelText('Quantity'), quantity);
    await user.type(screen.getByLabelText('Unitary cost'), cost);
    await user.type(screen.getByLabelText('User GUID'), userId);
};

describe('AddStockDialog', () => {
    let service;
    let onClose;
    let onStockAdded;

    const renderDialog = (item = ITEM) =>
        render(<AddStockDialog item={item} onClose={onClose} onStockAdded={onStockAdded} />);

    beforeEach(() => {
        service = { addStock: vi.fn() };
        ItemService.mockReturnValue(service);
        onClose = vi.fn();
        onStockAdded = vi.fn();
    });

    it('stays closed when no item is selected', () => {
        renderDialog(null);

        expect(getDialog()).not.toHaveAttribute('open');
    });

    it('opens for the selected item and focuses the quantity field', () => {
        renderDialog();

        expect(getDialog()).toHaveAttribute('open');
        expect(screen.getByRole('heading', { name: 'Add stock: Pen' })).toBeInTheDocument();
        expect(screen.getByLabelText('Quantity')).toHaveFocus();
    });

    it('does not submit while a required field is empty', async () => {
        const user = userEvent.setup();
        renderDialog();

        await user.type(screen.getByLabelText('Quantity'), '5');
        await user.click(screen.getByRole('button', { name: 'Add stock' }));

        expect(service.addStock).not.toHaveBeenCalled();
    });

    it('adds the stock with numeric values, closes and notifies the parent', async () => {
        service.addStock.mockResolvedValue({});
        const user = userEvent.setup();
        renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Add stock' }));

        await waitFor(() => expect(onStockAdded).toHaveBeenCalledTimes(1));
        expect(service.addStock).toHaveBeenCalledWith('item-1', {
            userCreatorId: 'user-1',
            quantity: 5,
            unitaryCost: 2.5,
        });
        expect(getDialog()).not.toHaveAttribute('open');
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it('disables the submit button while the request is pending', async () => {
        const pending = deferred();
        service.addStock.mockReturnValue(pending.promise);
        const user = userEvent.setup();
        renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Add stock' }));

        expect(screen.getByRole('button', { name: 'Add stock' })).toBeDisabled();
        pending.resolve({});
        await waitFor(() => expect(onStockAdded).toHaveBeenCalled());
    });

    it('shows a client message and keeps the dialog open when the API rejects', async () => {
        service.addStock.mockRejectedValue({
            response: { status: 400, data: { code: 'CreatorNotFound' } },
        });
        const user = userEvent.setup();
        renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Add stock' }));

        expect(await screen.findByRole('alert')).toHaveTextContent(
            'The user ID does not exist. Check it and try again.',
        );
        expect(getDialog()).toHaveAttribute('open');
        expect(screen.getByRole('button', { name: 'Add stock' })).toBeEnabled();
        expect(onStockAdded).not.toHaveBeenCalled();
        expect(onClose).not.toHaveBeenCalled();
    });

    it('falls back to a generic message for an unknown error', async () => {
        service.addStock.mockRejectedValue(new Error('Network Error'));
        const user = userEvent.setup();
        renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Add stock' }));

        expect(await screen.findByRole('alert')).toHaveTextContent('Stock could not be added');
    });

    it('closes and resets the form on Cancel without adding stock', async () => {
        const user = userEvent.setup();
        const { rerender } = renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Cancel' }));

        expect(getDialog()).not.toHaveAttribute('open');
        expect(onClose).toHaveBeenCalledTimes(1);
        expect(service.addStock).not.toHaveBeenCalled();

        rerender(<AddStockDialog item={null} onClose={onClose} onStockAdded={onStockAdded} />);
        rerender(<AddStockDialog item={ITEM} onClose={onClose} onStockAdded={onStockAdded} />);
        expect(screen.getByLabelText('Quantity')).toHaveValue(null);
        expect(screen.getByLabelText('Unitary cost')).toHaveValue(null);
        expect(screen.getByLabelText('User GUID')).toHaveValue('');
    });

    it('stays open and shows the new item when the selection changes while open', () => {
        const { rerender } = renderDialog();

        rerender(
            <AddStockDialog
                item={{ ...ITEM, id: 'item-2', itemName: 'Book' }}
                onClose={onClose}
                onStockAdded={onStockAdded}
            />,
        );

        expect(getDialog()).toHaveAttribute('open');
        expect(screen.getByRole('heading', { name: 'Add stock: Book' })).toBeInTheDocument();
        expect(onClose).not.toHaveBeenCalled();
    });

    it('closes when the parent clears the selected item', () => {
        const { rerender } = renderDialog();

        rerender(<AddStockDialog item={null} onClose={onClose} onStockAdded={onStockAdded} />);

        expect(getDialog()).not.toHaveAttribute('open');
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it('ignores the error of a request that finishes after the dialog was closed', async () => {
        const pending = deferred();
        service.addStock.mockReturnValue(pending.promise);
        const user = userEvent.setup();
        renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Add stock' }));
        await user.click(screen.getByRole('button', { name: 'Cancel' }));
        pending.reject(new Error('Network Error'));

        await waitFor(() => expect(onClose).toHaveBeenCalledTimes(1));
        expect(screen.queryByRole('alert', { hidden: true })).not.toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Add stock', hidden: true })).toBeEnabled();
    });

    it('still refreshes the list when a late request succeeds after closing', async () => {
        const pending = deferred();
        service.addStock.mockReturnValue(pending.promise);
        const user = userEvent.setup();
        renderDialog();

        await fillForm(user);
        await user.click(screen.getByRole('button', { name: 'Add stock' }));
        await user.click(screen.getByRole('button', { name: 'Cancel' }));
        pending.resolve({});

        await waitFor(() => expect(onStockAdded).toHaveBeenCalledTimes(1));
        expect(onClose).toHaveBeenCalledTimes(1);
    });
});
