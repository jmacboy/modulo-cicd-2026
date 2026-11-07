// Validate every message, except the ones git generates for its own workflows.
const allowedGitMessages = [
    // `git commit --fixup/--squash`, consumed by `git rebase --autosquash`.
    /^(amend|fixup|squash)! /,
    // `git revert`: `Revert "<subject>"` (or `Reapply "<subject>"`) plus the reverted SHA.
    /^(Revert|Reapply) ".+"\r?\n\r?\nThis reverts commit [0-9a-f]{7,40}/,
];

export default {
    extends: ['@commitlint/config-conventional'],
    defaultIgnores: false,
    ignores: allowedGitMessages.map((pattern) => (message) => pattern.test(message)),
};
