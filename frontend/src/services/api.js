const BASE = process.env.REACT_APP_API_URL;

export async function register(username, email, password) {
  const resp = await fetch(`${BASE}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, email, password })
  });
  if (!resp.ok) throw new Error(await resp.text());
  return await resp.json();
}

export async function loginRequest(username, password) {
  const resp = await fetch(`${BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password })
  });
  if (!resp.ok) throw new Error(await resp.text());
  const data = await resp.json();
  return data.token; // предполагаем `{ token: "..." }`
}

export async function generateText(token, prompt) {
  const rpc = {
    jsonrpc: '2.0',
    id: Date.now().toString(),
    method: 'GenerateText',
    params: { prompt }
  };
  const resp = await fetch(`${BASE}/llm`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify(rpc)
  });
  if (!resp.ok) throw new Error(await resp.text());
  const data = await resp.json();
  if (data.error) throw new Error(data.error.message);
  return data.result.text;
}
