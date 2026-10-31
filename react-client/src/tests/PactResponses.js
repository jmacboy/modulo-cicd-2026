import { MatchersV3 } from '@pact-foundation/pact';

const { like, eachLike, uuid, integer } = MatchersV3;

export const itemDtoTemplate = {
    id: uuid('0c7a4eee-ab9c-4431-8de7-beb6559e5c4e'),
    itemName: like('item1'),
    stock: integer(10),
    available: integer(8),
    reserved: integer(2),
    unitaryCost: like(12.5),
};
export const crearItemRequestBody = {
    id: '1a2b3c4d-5678-90ab-cdef-1234567890ab',
    itemName: 'nuevoItem',
};

export const responseItemList = {
    value: eachLike(itemDtoTemplate, 1),
};
export const crearItemResponse = {
    value: uuid('1a2b3c4d-5678-90ab-cdef-1234567890ab'),
};

export const itemIdToDelete = '2b3c4d5e-6f70-8192-a3b4-c5d6e7f8a9b0';

export const eliminarItemResponse = {
    value: like(true),
};
