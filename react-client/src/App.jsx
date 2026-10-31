import { useState } from 'react';
import './App.css';
import ItemList from './ItemList';
import { ItemService } from './services/ItemService';

function App() {
    const [guid, setGuid] = useState('');
    const [itemName, setItemName] = useState('');
    const [reloadCount, setReloadCount] = useState(1);
    const onFormSubmit = (e) => {
        e.preventDefault();
        ItemService()
            .addItem({ id: guid, itemName })
            .then(() => {
                setGuid('');
                setItemName('');
                setReloadCount(reloadCount + 1);
            });
    };
    return (
        <>
            <form onSubmit={onFormSubmit}>
                <input
                    type="text"
                    value={guid}
                    required
                    onChange={(e) => setGuid(e.target.value)}
                    placeholder="Enter GUID"
                />
                <input
                    type="text"
                    value={itemName}
                    required
                    onChange={(e) => setItemName(e.target.value)}
                    placeholder="Enter Item Name"
                />
                <button type="submit">Submit</button>
            </form>
            <ItemList reload={reloadCount} />
        </>
    );
}

export default App;
