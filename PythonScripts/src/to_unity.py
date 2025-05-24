from stable_baselines3 import PPO
from unity_wrapper import UnityCarrierWrapper

# Инициализация обертки
env = UnityCarrierWrapper("../environment/carrier_robot_gym/carrier_robot_gym.py")

# Загрузка модели
model = PPO.load("../models/CarrierRobot v61 5x5 240m.zip")

# Запуск предсказаний
obs, _ = env.reset()
while True:
    action, _ = model.predict(obs, deterministic=True)
    obs, reward, done, _, _ = env.step(action)

    if done:
        obs, _ = env.reset()