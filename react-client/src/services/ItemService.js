import axios from 'axios';
export const ItemService = (baseUrl = 'https://localhost:7297') => {
    return {
        getItemList: () => {
            return new Promise((resolve, reject) => {
                console.log('Fetching item list from API...');
                axios
                    .get(`${baseUrl}/api/Item`, {
                        headers: {
                            'Content-Type': 'application/json',
                        },
                    })
                    .then((response) => {
                        resolve(response.data.value);
                    })
                    .catch((error) => {
                        reject(error);
                    });
            });
        },
        addItem: (item) => {
            return new Promise((resolve, reject) => {
                axios
                    .post(`${baseUrl}/api/Item`, item, {
                        headers: {
                            'Content-Type': 'application/json',
                        },
                    })
                    .then((response) => {
                        resolve(response.data);
                    })
                    .catch((error) => {
                        reject(error);
                    });
            });
        },
        deleteItem: (id) => {
            return new Promise((resolve, reject) => {
                axios
                    .delete(`${baseUrl}/api/Item/${id}`)
                    .then((response) => {
                        resolve(response.data);
                    })
                    .catch((error) => {
                        reject(error);
                    });
            });
        },
        updateItem: (id, item) => {
            return new Promise((resolve, reject) => {
                axios
                    .put(`${baseUrl}/api/Item/${id}`, item, {
                        headers: {
                            'Content-Type': 'application/json',
                        },
                    })
                    .then((response) => {
                        resolve(response.data);
                    })
                    .catch((error) => {
                        reject(error);
                    });
            });
        },
    };
};
