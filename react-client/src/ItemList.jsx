import { useEffect, useState } from 'react';
import { ItemService } from './services/ItemService';

const ItemList = ({ reload }) => {
    const [itemList, setItemList] = useState();

    useEffect(() => {
        ItemService()
            .getItemList()
            .then((items) => {
                setItemList(items);
            });
    }, [reload]);

    const handleDelete = (id) => {
        ItemService()
            .deleteItem(id)
            .then(() => {
                setItemList((curr) => curr.filter((item) => item.id !== id));
            });
    };

    return (
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
                                <button type="button" onClick={() => handleDelete(item.id)}>
                                    Delete
                                </button>
                            </td>
                        </tr>
                    ))}
            </tbody>
        </table>
    );
};

export default ItemList;
