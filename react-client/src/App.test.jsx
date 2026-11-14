import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { ItemService } from './services/ItemService';

vi.mock('./services/ItemService', () => ({ ItemService: vi.fn() }));

describe('App', () => {
    let service;

    beforeEach(() => {
        service = {
            getItemList: vi.fn().mockResolvedValue([]),
            addItem: vi.fn().mockResolvedValue('guid-1'),
        };
        ItemService.mockReturnValue(service);
    });

    const fillAndSubmit = async (user, guid, name) => {
        await user.type(screen.getByPlaceholderText('Enter GUID'), guid);
        await user.type(screen.getByPlaceholderText('Enter Item Name'), name);
        await user.click(screen.getByRole('button', { name: 'Submit' }));
    };

    it('renders the create form and the item list', async () => {
        render(<App />);

        expect(screen.getByPlaceholderText('Enter GUID')).toBeInTheDocument();
        expect(screen.getByPlaceholderText('Enter Item Name')).toBeInTheDocument();
        expect(screen.getByRole('table')).toBeInTheDocument();
        await waitFor(() => expect(service.getItemList).toHaveBeenCalledTimes(1));
    });

    it('does not submit while a required field is empty', async () => {
        const user = userEvent.setup();
        render(<App />);

        await user.type(screen.getByPlaceholderText('Enter GUID'), 'guid-1');
        await user.click(screen.getByRole('button', { name: 'Submit' }));

        expect(service.addItem).not.toHaveBeenCalled();
    });

    it('creates the item, clears the form and reloads the list', async () => {
        const user = userEvent.setup();
        render(<App />);

        await fillAndSubmit(user, 'guid-1', 'Pen');

        expect(service.addItem).toHaveBeenCalledWith({ id: 'guid-1', itemName: 'Pen' });
        await waitFor(() => expect(service.getItemList).toHaveBeenCalledTimes(2));
        expect(screen.getByPlaceholderText('Enter GUID')).toHaveValue('');
        expect(screen.getByPlaceholderText('Enter Item Name')).toHaveValue('');
        expect(screen.queryByRole('alert')).not.toBeInTheDocument();
    });

    it('keeps the input and shows the mapped message when creation fails', async () => {
        service.addItem.mockRejectedValue({
            response: { status: 400, data: { code: 'NameIsRequired' } },
        });
        const user = userEvent.setup();
        render(<App />);

        await fillAndSubmit(user, 'guid-1', 'Pen');

        expect(await screen.findByRole('alert')).toHaveTextContent('Enter a name for the item.');
        expect(screen.getByPlaceholderText('Enter GUID')).toHaveValue('guid-1');
        expect(service.getItemList).toHaveBeenCalledTimes(1);
    });

    it('falls back to a generic message and clears it on the next successful submit', async () => {
        service.addItem.mockRejectedValueOnce(new Error('Network Error'));
        const user = userEvent.setup();
        render(<App />);

        await fillAndSubmit(user, 'guid-1', 'Pen');
        expect(await screen.findByRole('alert')).toHaveTextContent('The item could not be created');

        await user.click(screen.getByRole('button', { name: 'Submit' }));

        await waitFor(() => expect(screen.queryByRole('alert')).not.toBeInTheDocument());
        expect(service.addItem).toHaveBeenCalledTimes(2);
    });
});
