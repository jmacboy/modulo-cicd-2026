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

export const itemIdToUpdate = '3c4d5e6f-7081-92a3-b4c5-d6e7f8a9b0c1';
export const unknownItemIdToUpdate = '4d5e6f70-8192-a3b4-c5d6-e7f8a9b0c1d2';

export const actualizarItemRequestBody = {
    name: 'itemActualizado',
};

export const actualizarItemNombreVacioRequestBody = {
    name: '',
};

export const actualizarItemResponse = {
    value: like(true),
};

// The provider answers 404 with a text/plain body. PactV3 matchers only apply to JSON bodies
// (a matcher here is sent as a serialized JSON object), so this body has to be a plain string.
export const actualizarItemNoEncontradoResponse = `Item with id ${unknownItemIdToUpdate} not found`;

export const actualizarItemNombreRequeridoResponse = {
    code: like('NameIsRequired'),
    description: like('The item name is required'),
    type: integer(2),
};
