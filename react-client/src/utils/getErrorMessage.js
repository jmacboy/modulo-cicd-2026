/**
 * Client-owned messages for the error codes returned by the inventory API.
 * Backend descriptions are never shown to the user.
 */
const MESSAGES_BY_CODE = {
    ItemNotFound: 'The item no longer exists. Refresh the list and try again.',
    NameIsRequired: 'Enter a name for the item.',
    ItemAlreadyDeleted: 'The item was already deleted.',
    CannotUpdateDeletedItem: 'A deleted item cannot be updated.',
    CannotDeleteWithReservedStock: 'The item has reserved stock and cannot be deleted.',
    NonNegativeStock: 'The quantity must be greater than zero.',
    NonNegativeCost: 'The unitary cost cannot be negative.',
    CreatorIsRequired: 'Enter the user ID that adds the stock.',
    InsuficentStockToReserve: 'There is not enough available stock.',
    InsuficentStockToSubstract: 'There is not enough reserved stock.',
};

const MESSAGES_BY_STATUS = {
    404: MESSAGES_BY_CODE.ItemNotFound,
};

/**
 * Maps an axios error from the inventory API to a client-owned, user-facing message.
 * The API returns an `Error` object (`code`) for 400 and a plain string for 404; neither text
 * reaches the UI. Unknown errors fall back to the action-specific message.
 */
export const getErrorMessage = (error, fallback) => {
    const code = error?.response?.data?.code;
    const status = error?.response?.status;
    return MESSAGES_BY_CODE[code] ?? MESSAGES_BY_STATUS[status] ?? fallback;
};
