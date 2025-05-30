FROM python:3.10-slim

# 1) Устанавливаем системные зависимости для сборки llama-cpp-python
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
      build-essential \
      cmake \
      git \
      libopenblas-dev \
      libomp-dev \
      ccache \
 && rm -rf /var/lib/apt/lists/*

RUN pip install --no-cache-dir --upgrade pip \
 && pip install --no-cache-dir "llama-cpp-python[server]==0.1.78"

# 3) Копируем модель внутрь контейнера
WORKDIR /models
COPY open-llama-7B-open-instruct.ggmlv3.q4_0.bin .

# 4) Экспонируем порт и запускаем HTTP API
EXPOSE 8000
CMD ["python3", "-m", "llama_cpp.server", \
     "--model", "/models/open-llama-7B-open-instruct.ggmlv3.q4_0.bin", \
     "--host", "0.0.0.0", "--port", "8000"]
