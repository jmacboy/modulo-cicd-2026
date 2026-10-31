#!/usr/bin/env node
// UserPromptSubmit hook: reinforces the "delegate test generation to test-writer"
// rule from CLAUDE.md at the moment the request lands, instead of relying only
// on the model reading the instructions file.
let data = "";
process.stdin.on("data", (chunk) => (data += chunk));
process.stdin.on("end", () => {
  let input;
  try {
    input = JSON.parse(data);
  } catch {
    process.exit(0);
  }

  const prompt = input.prompt || "";
  const trigger =
    /(generate|write|create|add)\s+(unit\s+|integration\s+)?tests?\b|test\s+this(\s+(file|class|folder|controller))?\b|\b(genera|generar|crea|crear|escribe|escribir|haz|agrega|añade|agregar)\s+(un\s+|unos\s+)?tests?\b|\btests?\s+(unitarios|de\s+integraci[oó]n|de|para)\b|\bintegration\s+tests?\b/i;

  if (!trigger.test(prompt)) {
    process.exit(0);
  }

  const isIntegration =
    /\bintegration\b|\bintegraci[oó]n\b|\bcontroller\b|\bend[- ]to[- ]end\b|\be2e\b|\bhttp\b/i.test(
      prompt
    );
  const isUnit = /\bunit\b|\bunitari[oa]s?\b/i.test(prompt);

  let hint;
  if (isIntegration && !isUnit) {
    hint = "Por el enunciado, esto parece un pedido de tests de INTEGRACION.";
  } else if (isUnit && !isIntegration) {
    hint = "Por el enunciado, esto parece un pedido de tests UNITARIOS.";
  } else {
    hint =
      "El enunciado no deja claro si son tests unitarios o de integracion (o mezcla ambos).";
  }

  const context =
    "Regla del proyecto (CLAUDE.md): esta solicitud pide crear/generar/verificar tests. " +
    hint +
    ' NO escribas ni edites archivos de test directamente en el hilo principal. Debes delegar ' +
    'al subagente "test-writer" usando la herramienta Agent (subagent_type: "test-writer"), ' +
    "pasandole el archivo, clase o carpeta objetivo. Ese agente decide el track (unitario vs " +
    "integracion) segun el pedido y el tipo de archivo, y ejecuta generacion + verificacion + " +
    "correccion en un solo flujo (test-generator/test-verifier para unitarios, " +
    "integration-generator/integration-verifier para integracion).";

  console.log(
    JSON.stringify({
      hookSpecificOutput: {
        hookEventName: "UserPromptSubmit",
        additionalContext: context,
      },
    })
  );
});
