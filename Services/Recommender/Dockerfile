FROM python:3.12.5

WORKDIR /code

COPY Services/Recommender/requirements.txt /code/requirements.txt

RUN pip install --no-cache-dir --upgrade -r /code/requirements.txt

COPY Services/Recommender/data /code/data
COPY Services/Recommender/model /code/model
COPY Services/Recommender/output /code/output
COPY Services/Recommender/main.py /code/main.py

CMD ["fastapi", "run", "main.py", "--port", "80"]