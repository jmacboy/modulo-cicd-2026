#!/usr/bin/env node
// UserPromptSubmit hook: routes Pact/contract-test requests to the
// pact-workflow subagent instead of letting Claude handle them ad hoc.
let data = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', (chunk) => {
    data += chunk;
});
process.stdin.on('end', () => {
    let input;
    try {
        input = JSON.parse(data || '{}');
    } catch {
        process.exit(0);
    }

    const prompt = input.prompt || '';
    if (!/pact|contract[-\s]?test/i.test(prompt)) {
        process.exit(0);
    }

    process.stdout.write(
        JSON.stringify({
            hookSpecificOutput: {
                hookEventName: 'UserPromptSubmit',
                additionalContext:
                    'This request is about a Pact/contract test in react-client. ' +
                    'Use the Agent tool with the project subagent "pact-workflow" ' +
                    '(defined at .claude/agents/pact-workflow.md) for this request ' +
                    'instead of handling it ad hoc — it generates the test via ' +
                    'pact-generator and immediately verifies it via pact-checker in ' +
                    'one pass.',
            },
        }),
    );
});
