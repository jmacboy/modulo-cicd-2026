import { useState } from 'react';
import { ItemService } from './services/ItemService';
import { getErrorMessage } from './utils/getErrorMessage';

const AddStockForm = ({ itemId, itemName, onStockAdded }) => {
    const [quantity, setQuantity] = useState('');
    const [unitaryCost, setUnitaryCost] = useState('');
    const [userCreatorId, setUserCreatorId] = useState('');
    const [error, setError] = useState(null);
    const [submitting, setSubmitting] = useState(false);

    const handleSubmit = (e) => {
        e.preventDefault();
        setError(null);
        setSubmitting(true);
        ItemService()
            .addStock(itemId, {
                userCreatorId,
                quantity: Number(quantity),
                unitaryCost: Number(unitaryCost),
            })
            .then(() => {
                setQuantity('');
                setUnitaryCost('');
                setUserCreatorId('');
                onStockAdded();
            })
            .catch((err) => {
                setError(getErrorMessage(err, 'Stock could not be added'));
            })
            .finally(() => {
                setSubmitting(false);
            });
    };

    const fieldId = (name) => `add-stock-${itemId}-${name}`;

    return (
        <form
            className="add-stock-form"
            onSubmit={handleSubmit}
            aria-label={`Add stock to ${itemName}`}
        >
            <label htmlFor={fieldId('quantity')}>Quantity</label>
            <input
                id={fieldId('quantity')}
                type="number"
                min="1"
                step="1"
                required
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
            />
            <label htmlFor={fieldId('cost')}>Unitary cost</label>
            <input
                id={fieldId('cost')}
                type="number"
                min="0"
                step="0.01"
                required
                value={unitaryCost}
                onChange={(e) => setUnitaryCost(e.target.value)}
            />
            <label htmlFor={fieldId('user')}>User GUID</label>
            <input
                id={fieldId('user')}
                type="text"
                required
                value={userCreatorId}
                onChange={(e) => setUserCreatorId(e.target.value)}
            />
            <button type="submit" disabled={submitting}>
                Add stock
            </button>
            {error && <p role="alert">{error}</p>}
        </form>
    );
};

export default AddStockForm;
