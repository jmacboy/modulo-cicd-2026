import { useEffect, useRef, useState } from 'react';
import { ItemService } from './services/ItemService';
import { getErrorMessage } from './utils/getErrorMessage';

// One modal for the whole list: `item` is the selected item, or null when the dialog is closed.
// Every close path (Escape, Cancel, success) goes through the native `close` event, so the form
// is reset in one place and the parent is told the dialog is gone. There is no backdrop-click
// close: a click listener on the non-interactive <dialog> breaks code rules JS-5 and JS-6.
// Each submission is tagged with an id; closing the dialog invalidates it, so a request that
// resolves after the user closed (or reopened the dialog for another item) never touches the
// new session's state.
const AddStockDialog = ({ item, onClose, onStockAdded }) => {
    const dialogRef = useRef(null);
    const quantityRef = useRef(null);
    const [quantity, setQuantity] = useState('');
    const [unitaryCost, setUnitaryCost] = useState('');
    const [userCreatorId, setUserCreatorId] = useState('');
    const [error, setError] = useState(null);
    const [submitting, setSubmitting] = useState(false);
    const submissionIdRef = useRef(0);

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
        setSubmitting(false);
        submissionIdRef.current += 1;
        onClose();
    };

    const handleSubmit = (event) => {
        event.preventDefault();
        setError(null);
        setSubmitting(true);
        submissionIdRef.current += 1;
        const submissionId = submissionIdRef.current;
        const isCurrent = () => submissionIdRef.current === submissionId;
        ItemService()
            .addStock(item.id, {
                userCreatorId,
                quantity: Number(quantity),
                unitaryCost: Number(unitaryCost),
            })
            .then(() => {
                // The stock was added even if the user closed the dialog meanwhile: refresh anyway.
                if (isCurrent()) {
                    closeDialog();
                }
                onStockAdded();
            })
            .catch((err) => {
                if (isCurrent()) {
                    setError(getErrorMessage(err, 'Stock could not be added'));
                }
            })
            .finally(() => {
                if (isCurrent()) {
                    setSubmitting(false);
                }
            });
    };

    return (
        <dialog
            ref={dialogRef}
            className="add-stock-dialog"
            aria-labelledby="add-stock-title"
            onClose={handleClose}
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
