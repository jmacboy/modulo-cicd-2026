import { PactV3 } from '@pact-foundation/pact';
import { describe, it } from 'mocha';
import { expect } from 'chai';
import { ItemService } from '../services/ItemService.js';
import {
    crearItemRequestBody,
    crearItemResponse,
    responseItemList,
    itemIdToDelete,
    eliminarItemResponse,
} from './PactResponses.js';

describe('El API de Items', () => {
    let itemService;
    const provider = new PactV3({
        consumer: 'react-client',
        provider: 'inventory-service',
    });
    describe('obtener lista de items', () => {
        it('retorna una lista de items', () => {
            //Arrange
            provider
                .given('realizar una consulta de items')
                .uponReceiving('Un body vacío')
                .withRequest({
                    method: 'GET',
                    path: '/api/Item',
                })
                .willRespondWith({
                    status: 200,
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: responseItemList,
                });
            return provider.executeTest(async (mockServer) => {
                // Act
                itemService = ItemService(mockServer.url);
                return itemService.getItemList().then((response) => {
                    // Assert — lo que el pacto promete: un array de al menos un item, cada uno
                    // con la forma real de ItemDto. No se afirma cuántos items hay ni cuáles son
                    // sus valores — eso lo decide el proveedor, no este consumidor.
                    expect(response).to.be.an('array').that.is.not.empty;

                    const [firstItem] = response;
                    expect(firstItem).to.have.all.keys(
                        'id',
                        'itemName',
                        'stock',
                        'available',
                        'reserved',
                        'unitaryCost',
                    );
                    expect(firstItem.itemName).to.be.a('string');
                    expect(firstItem.stock).to.be.a('number');
                    expect(firstItem.available).to.be.a('number');
                    expect(firstItem.reserved).to.be.a('number');
                    expect(firstItem.unitaryCost).to.be.a('number');
                });
            });
        });
    });
    describe('Agregar un item', () => {
        it('retorna un id de item ya creado', () => {
            //Arrange
            provider
                .given('crear item', { id: crearItemRequestBody.id })
                .uponReceiving('datos para crear un item')
                .withRequest({
                    method: 'POST',
                    path: '/api/Item',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: crearItemRequestBody,
                })
                .willRespondWith({
                    status: 200,
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: crearItemResponse,
                });
            return provider.executeTest(async (mockServer) => {
                // Act
                itemService = ItemService(mockServer.url);
                return itemService.addItem(crearItemRequestBody).then((response) => {
                    // Assert — el pacto promete "vuelve un id con forma de GUID", no "vuelve el
                    // mismo id que mandé": esa igualdad era casualidad del fixture, no una
                    // garantía real del contrato. Afirmar la forma es lo que de verdad se puede
                    // sostener acá.
                    expect(response.value).to.be.a('string');
                    expect(response.value).to.match(
                        /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i,
                    );
                });
            });
        });
    });
    describe('Eliminar un item', () => {
        it('confirma la eliminación de un item existente', () => {
            //Arrange
            provider
                .given('eliminar item', { id: itemIdToDelete })
                .uponReceiving('una solicitud para eliminar un item existente')
                .withRequest({
                    method: 'DELETE',
                    path: `/api/Item/${itemIdToDelete}`,
                })
                .willRespondWith({
                    status: 200,
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: eliminarItemResponse,
                });
            return provider.executeTest(async (mockServer) => {
                // Act
                itemService = ItemService(mockServer.url);
                return itemService.deleteItem(itemIdToDelete).then((response) => {
                    // Assert — el pacto promete que la respuesta trae un campo "value" booleano
                    // confirmando el resultado de la operación, no que ese valor sea
                    // necesariamente `true`: eso lo decide el proveedor, no este consumidor.
                    expect(response).to.have.all.keys('value');
                    expect(response.value).to.be.a('boolean');
                });
            });
        });
    });
});
