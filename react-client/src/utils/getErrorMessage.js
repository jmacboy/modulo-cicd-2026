/**
 * Extracts a user-facing message from an axios error returned by the inventory API.
 * The API returns a plain string for 404 and an `Error` object (`description`) for 400.
 */
export const getErrorMessage = (error, fallback) => {
    const data = error.response?.data;
    return typeof data === 'string' ? data : (data?.description ?? fallback);
};
