import { useEffect, useRef, useState } from 'react';
import { ItemService } from './services/ItemService';
import { getErrorMessage } from './utils/getErrorMessage';

// One modal for the whole list: `item` is the selected item, or null when the dialog is closed.
// Every close path (Escape, Cancel, backdrop click, success) goes through the native `close`
// event, so the form is reset in one place and the parent is told the dialog is gone.
const AddStockDialog = ({ item, onClose, onStockAdded }) => {
    const dialogRef = useRef(null);
    const quantityRef = useRef(null);
    const [quantity, setQuantity] = useState('');
    const [unitaryCost, setUnitaryCost] = useState('');
    const [userCreatorId, setUserCreatorId] = useState('');
    const [error, setError] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        const dialog = dialogRef.current;
        if (item) {
            if (!dialog.open) {
                dialog.showModal();
            }
            // The user just clicked "Add stock", so moving focus into the dialog is expected.
            quantityRef.current?.focus();
        } else if (dialog.open) {
            dialog.close();
        }
    }, [item]);

    const closeDialog = () => {
        dialogRef.current?.close();
    };

    const handleClose = () => {
        setQuantity('');
        setUnitaryCost('');
        setUserCreatorId('');
        setError(null);
        onClose();
    };

    const handleBackdropClick = (event) => {
        // The dialog has no padding, so a click whose target is the dialog itself hit the backdrop.
        if (event.target === dialogRef.current) {
            closeDialog();
        }
    };

    const handleSubmit = (event) => {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        ItemService()
            .addStock(item.id, {
                userCreatorId,
                quantity: Number(quantity),
                unitaryCost: Number(unitaryCost),
            })
            .then(() => {
                closeDialog();
                onStockAdded();
            })
            .catch((err) => {
                // Ignore failures that arrive after the user already closed the dialog.
                if (dialogRef.current?.open) {
                    setError(getErrorMessage(err, 'Stock could not be added'));
                }
            })
            .finally(() => {
                setSubmitting(false);
            });
    };

    return (
        <dialog
            ref={dialogRef}
            className="add-stock-dialog"
            aria-labelledby="add-stock-title"
            onClose={handleClose}
            onClick={handleBackdropClick}
        >
            <form className="add-stock-form" onSubmit={handleSubmit}>
                <h2 id="add-stock-title">Add stock: {item?.itemName}</h2>
                <label htmlFor="add-stock-quantity">Quantity</label>
                <input
                    id="add-stock-quantity"
                    ref={quantityRef}
                    type="number"
                    min="1"
                    step="1"
                    required
                    value={quantity}
                    onChange={(event) => setQuantity(event.target.value)}
                />
                <label htmlFor="add-stock-cost">Unitary cost</label>
                <input
                    id="add-stock-cost"
                    type="number"
                    min="0"
                    step="0.01"
                    required
                    value={unitaryCost}
                    onChange={(event) => setUnitaryCost(event.target.value)}
                />
                <label htmlFor="add-stock-user">User GUID</label>
                <input
                    id="add-stock-user"
                    type="text"
                    required
                    value={userCreatorId}
                    onChange={(event) => setUserCreatorId(event.target.value)}
                />
                {error && <p role="alert">{error}</p>}
                <div className="add-stock-actions">
                    <button type="button" onClick={closeDialog}>
                        Cancel
                    </button>
                    <button type="submit" disabled={submitting}>
                        Add stock
                    </button>
                </div>
            </form>
        </dialog>
    );
};

export default AddStockDialog;
