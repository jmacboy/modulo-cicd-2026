import { useEffect, useRef, useState } from 'react';
import { ItemService } from './services/ItemService';
import AddStockDialog from './AddStockDialog';
import { getErrorMessage } from './utils/getErrorMessage';

const iconProps = {
    xmlns: 'http://www.w3.org/2000/svg',
    width: '16',
    height: '16',
    viewBox: '0 0 24 24',
    fill: 'none',
    stroke: 'currentColor',
    strokeWidth: '2',
    strokeLinecap: 'round',
    strokeLinejoin: 'round',
    'aria-hidden': 'true',
    focusable: 'false',
};

const TrashIcon = () => (
    <svg {...iconProps}>
        <path d="M3 6h18" />
        <path d="M8 6V4h8v2" />
        <path d="M19 6l-1 14H6L5 6" />
        <path d="M10 11v6M14 11v6" />
    </svg>
);

const PencilIcon = () => (
    <svg {...iconProps}>
        <path d="M12 20h9" />
        <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4Z" />
    </svg>
);

const buttonStyle = {
    display: 'inline-flex',
    alignItems: 'center',
    gap: '0.25rem',
};

const ItemList = ({ reload }) => {
    const [itemList, setItemList] = useState([]);
    const [error, setError] = useState(null);
    const [editingId, setEditingId] = useState(null);
    const [editName, setEditName] = useState('');
    const [refreshCount, setRefreshCount] = useState(0);
    const [stockItem, setStockItem] = useState(null);
    const editInputRef = useRef(null);

    // Move focus to the name input only after the user clicks Edit (user-initiated focus change).
    useEffect(() => {
        if (editingId !== null) {
            editInputRef.current?.focus();
        }
    }, [editingId]);

    useEffect(() => {
        ItemService()
            .getItemList()
            .then((items) => {
                setItemList(items ?? []);
            })
            .catch((err) => {
                setError(getErrorMessage(err, 'The item list could not be loaded'));
            });
    }, [reload, refreshCount]);

    const handleDelete = (id) => {
        setError(null);
        ItemService()
            .deleteItem(id)
            .then(() => {
                setItemList((curr) => curr.filter((item) => item.id !== id));
            })
            .catch((err) => {
                setError(getErrorMessage(err, 'The item could not be deleted'));
            });
    };

    const startEdit = (item) => {
        setError(null);
        setEditingId(item.id);
        setEditName(item.itemName);
    };

    const cancelEdit = () => {
        setEditingId(null);
        setEditName('');
    };

    const handleSave = (id) => {
        if (!editName.trim()) {
            return;
        }
        setError(null);
        ItemService()
            .updateItem(id, { name: editName })
            .then(() => {
                setItemList((curr) =>
                    curr.map((item) => (item.id === id ? { ...item, itemName: editName } : item)),
                );
                cancelEdit();
            })
            .catch((err) => {
                setError(getErrorMessage(err, 'The item could not be updated'));
            });
    };

    const handleEditKeyDown = (event, id) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleSave(id);
        } else if (event.key === 'Escape') {
            cancelEdit();
        }
    };

    return (
        <>
            {error && <p role="alert">{error}</p>}
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                        <th>Stock</th>
                        <th>Available</th>
                        <th>Unitary cost</th>
                        <th>Add stock</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {itemList.map((item) =>
                        item.id === editingId ? (
                            <tr key={item.id}>
                                <td>{item.id}</td>
                                <td>
                                    <input
                                        aria-label="Item name"
                                        value={editName}
                                        onChange={(event) => setEditName(event.target.value)}
                                        onKeyDown={(event) => handleEditKeyDown(event, item.id)}
                                        ref={editInputRef}
                                    />
                                </td>
                                <td>{item.stock}</td>
                                <td>{item.available}</td>
                                <td>{item.unitaryCost.toFixed(2)}</td>
                                <td></td>
                                <td>
                                    <button
                                        type="button"
                                        onClick={() => handleSave(item.id)}
                                        disabled={!editName.trim()}
                                    >
                                        Save
                                    </button>
                                    <button type="button" onClick={cancelEdit}>
                                        Cancel
                                    </button>
                                </td>
                            </tr>
                        ) : (
                            <tr key={item.id}>
                                <td>{item.id}</td>
                                <td>{item.itemName}</td>
                                <td>{item.stock}</td>
                                <td>{item.available}</td>
                                <td>{item.unitaryCost.toFixed(2)}</td>
                                <td>
                                    <button type="button" onClick={() => setStockItem(item)}>
                                        Add stock
                                    </button>
                                </td>
                                <td>
                                    <button
                                        type="button"
                                        onClick={() => startEdit(item)}
                                        style={buttonStyle}
                                    >
                                        <PencilIcon />
                                        Edit
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => handleDelete(item.id)}
                                        style={buttonStyle}
                                    >
                                        <TrashIcon />
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ),
                    )}
                </tbody>
            </table>
            <AddStockDialog
                item={stockItem}
                onClose={() => setStockItem(null)}
                onStockAdded={() => setRefreshCount((curr) => curr + 1)}
            />
        </>
    );
};

export default ItemList;
