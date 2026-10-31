import { useEffect, useState } from 'react';
import { ItemService } from './services/ItemService';

const TrashIcon = () => (
    <svg
        xmlns="http://www.w3.org/2000/svg"
        width="16"
        height="16"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
        focusable="false"
    >
        <path d="M3 6h18" />
        <path d="M8 6V4h8v2" />
        <path d="M19 6l-1 14H6L5 6" />
        <path d="M10 11v6M14 11v6" />
    </svg>
);

const ItemList = ({ reload }) => {
    const [itemList, setItemList] = useState();
    const [deleteError, setDeleteError] = useState(null);

    useEffect(() => {
        ItemService()
            .getItemList()
            .then((items) => {
                setItemList(items);
            });
    }, [reload]);

    const handleDelete = (id) => {
        setDeleteError(null);
        ItemService()
            .deleteItem(id)
            .then(() => {
                setItemList((curr) => curr.filter((item) => item.id !== id));
            })
            .catch((error) => {
                const data = error.response?.data;
                const message =
                    typeof data === 'string'
                        ? data
                        : (data?.description ?? 'The item could not be deleted');
                setDeleteError(message);
            });
    };

    return (
        <>
            {deleteError && <p role="alert">{deleteError}</p>}
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Name</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {itemList &&
                        itemList.map((item) => (
                            <tr key={item.id}>
                                <td>{item.id}</td>
                                <td>{item.itemName}</td>
                                <td>
                                    <button
                                        type="button"
                                        onClick={() => handleDelete(item.id)}
                                        style={{
                                            display: 'inline-flex',
                                            alignItems: 'center',
                                            gap: '0.25rem',
                                        }}
                                    >
                                        <TrashIcon />
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                </tbody>
            </table>
        </>
    );
};

export default ItemList;
