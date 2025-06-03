from flask import Flask, request, jsonify

app = Flask(__name__)

# Роут теперь совпадает с тем, что отсылает C#:
#   POST http://<ваш-хост>:8000/v1/completions
@app.route('/v1/completions', methods=['POST'])
def completions():
    # Получаем весь JSON из тела (model, prompt, max_tokens, temperature)
    data = request.get_json(force=True)

    # Извлекаем prompt (остальные поля можно игнорировать)
    prompt = data.get('prompt', '')

    # Формируем «заглушечный» ответ в формате:
    # {
    #   "choices": [
    #     { "text": "..." }
    #   ]
    # }
    fake_answer = f"LLM response for: {prompt}"
    response_body = {
        "choices": [
            { "text": fake_answer }
        ]
    }

    return jsonify(response_body), 200


if __name__ == '__main__':
    # Запустим на 0.0.0.0:8000, чтобы C#-код (http://llm:8000/v1/completions) попал именно сюда
    app.run(host='0.0.0.0', port=8000)
