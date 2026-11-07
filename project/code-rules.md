# Code Rules

Rules that production and test code must follow. Each rule has a stable ID so reviews, commits, and
static-analysis findings can reference it. Package rules (testing, Pact) live next to each package
and are linked from [README.md](README.md).

## JavaScript / React (`react-client`)

### JS-1: Handle every promise

**Source:** SonarQube `javascript:S9383`.

Every promise must be one of:

1. awaited (`await promise`) inside a `try`/`catch` or a function whose caller handles the rejection;
2. returned to a caller that handles it (`return promise.then(...)`, e.g. a test body returned to
   the test runner or to `provider.executeTest`);
3. ended with `.catch(...)`;
4. ended with `.then(onFulfilled, onRejected)` (a rejection handler as the second argument);
5. explicitly marked as fire-and-forget with the `void` operator (`void promise`), only when a
   rejection is truly harmless.

**Why:** an unhandled rejection fails silently: the UI stays in a stale or loading state and the
user never sees the error.

**Bad**

```jsx
useEffect(() => {
    ItemService()
        .getItemList()
        .then((items) => setItemList(items));
}, [reload]);
```

**Good**

```jsx
useEffect(() => {
    ItemService()
        .getItemList()
        .then((items) => setItemList(items))
        .catch((err) => setError(getErrorMessage(err, 'The item list could not be loaded')));
}, [reload]);
```

In React components, surface the failure to the user (for example, a `role="alert"` message) instead
of only logging it.

### JS-2: Prefer optional chaining over `&&` guards

**Source:** SonarQube `javascript:S6582`.

Use an optional chain (`a?.b`, `a?.()`, `a?.[i]`) instead of guarding a property access, call, or
index with `&&` on the same value.

**Why:** it is shorter, states the intent ("only if defined") directly, and avoids repeating the
same expression, which can drift when one side is edited and the other is not.

**Bad**

```jsx
{itemList && itemList.map((item) => <tr key={item.id}>...</tr>)}
const message = error.response && error.response.data;
```

**Good**

```jsx
{itemList?.map((item) => <tr key={item.id}>...</tr>)}
const message = error.response?.data;
```

Note: `a?.b` yields `undefined` when `a` is nullish, while `a && a.b` yields `a` itself (for
example `null`, `0`, or `''`). In JSX, React renders `0` and `''` text, so the optional chain is
also safer there.

### JS-3: Never dereference a value that can be `null` or `undefined`

**Source:** SonarQube `javascript:S2259`.

Do not access a property or call a method on a value that may be `null` or `undefined`. Remove the
nullable state at its source instead of guarding every use:

1. initialize state with a value of the right type (`useState([])` for a list, `useState('')` for
   text), never `useState()`;
2. normalize external data when it enters the component (`setItemList(items ?? [])`);
3. only when a value is legitimately optional, guard it explicitly (`?.`, `??`, or an early return).

**Why:** a `TypeError: Cannot read properties of undefined` crashes the component. With an
uninitialized list, a delete or update that resolves before the list loads calls
`undefined.filter(...)`.

**Bad**

```jsx
const [itemList, setItemList] = useState();
// ...
setItemList((curr) => curr.filter((item) => item.id !== id)); // curr may be undefined
```

**Good**

```jsx
const [itemList, setItemList] = useState([]);
// ...
.then((items) => setItemList(items ?? []))
// ...
setItemList((curr) => curr.filter((item) => item.id !== id));
```

Prefer fixing the source (rules 1 and 2) over sprinkling `?.` on every access: a value that is never
nullable needs no guard (see JS-2 for when a guard is the right tool).

### JS-4: Do not use the `autoFocus` attribute

**Source:** SonarQube `javascript:S9379`.

Never put `autoFocus` on an element. When focus must move, move it programmatically with a `ref`,
and only as a direct response to a user action (for example, clicking Edit opens an input and
focuses it).

**Why:** `autoFocus` moves focus without the user asking for it. Screen reader users lose their
reading position and context, keyboard users skip content, and on mobile it can pop the keyboard and
scroll the page unexpectedly.

**Bad**

```jsx
<input aria-label="Item name" value={editName} autoFocus />
```

**Good**

```jsx
const editInputRef = useRef(null);

// Focus only after the user clicks Edit.
useEffect(() => {
    if (editingId !== null) {
        editInputRef.current?.focus();
    }
}, [editingId]);

<input aria-label="Item name" value={editName} ref={editInputRef} />;
```

Do not move focus on page load or on data refresh. Moving focus into a control the user just opened
is expected; without it, keyboard users are left on a button that no longer exists.

### JS-5: Do not attach mouse or keyboard listeners to non-interactive elements

**Source:** SonarQube `javascript:S6847`.

Do not put `onClick`, `onMouseDown`, `onKeyDown`, `onKeyUp`, or similar handlers on non-interactive
elements (`div`, `span`, `p`, `li`, `td`, `dialog`, `section`, ...). Put the behavior on a native
interactive element (`<button>`, `<a href>`, `<input>`, `<select>`) or rely on the element's native
behavior (a `<dialog>` already closes on Escape; a `<form>` submits with Enter).

**Why:** assistive technologies do not announce a `div` or `dialog` as clickable, and keyboard users
cannot reach or trigger it. The behavior exists only for mouse users.

**Bad**

```jsx
<dialog ref={dialogRef} onClick={handleBackdropClick}>...</dialog>
<td onClick={() => setStockItem(item)}>{item.itemName}</td>
```

**Good**

```jsx
<dialog ref={dialogRef} onClose={handleClose}>
    ...
    <button type="button" onClick={closeDialog}>Cancel</button>
</dialog>
<td>
    <button type="button" onClick={() => setStockItem(item)}>Add stock</button>
</td>
```

### JS-6: Every visible click handler needs a keyboard equivalent

**Source:** SonarQube `javascript:S1082`.

A visible element with `onClick` must also be operable from the keyboard (`onKeyDown`/`onKeyUp`/
`onKeyPress`). Native interactive elements already satisfy this, so the fix is almost always to use
a `<button>` instead of adding key handlers to a non-interactive element (see JS-5). Only when a
custom widget is unavoidable, give it a `role`, `tabIndex={0}`, and a key handler that matches the
click behavior (Enter and Space).

**Why:** keyboard-only and screen reader users cannot trigger an action that responds only to the
mouse.

**Bad**

```jsx
<span onClick={handleDelete}>Delete</span>
```

**Good**

```jsx
<button type="button" onClick={handleDelete}>
    Delete
</button>
```
