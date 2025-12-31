import os
import urllib.parse  # <--- Import this library
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import sessionmaker
from sqlalchemy.ext.declarative import declarative_base
from dotenv import load_dotenv

load_dotenv()

# 1. URL Encode the credentials to handle special chars like '@'
db_user = urllib.parse.quote_plus(os.getenv('DB_USER'))
db_password = urllib.parse.quote_plus(os.getenv('DB_PASSWORD'))
db_host = os.getenv('DB_HOST')
db_port = os.getenv('DB_PORT')
db_name = os.getenv('DB_NAME')

# 2. Build the Safe Connection String
DATABASE_URL = (
    f"mysql+aiomysql://{db_user}:{db_password}@{db_host}:{db_port}/{db_name}"
)

engine = create_async_engine(DATABASE_URL, echo=True)
SessionLocal = sessionmaker(bind=engine, class_=AsyncSession, expire_on_commit=False)

Base = declarative_base()

# Dependency to get DB session
async def get_db():
    async with SessionLocal() as session:
        try:
            yield session
        finally:
            await session.close()