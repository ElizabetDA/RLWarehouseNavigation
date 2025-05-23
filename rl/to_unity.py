
from mlagents_envs.environment import UnityEnvironment
from stable_baselines3 import PPO
import numpy as np


def connect_unity(path_unity_environment: str, path_model: str):

    # Загрузка модели
    model = PPO.load(path_model)

    # Подключение к Unity
    env = UnityEnvironment(file_name=path_unity_environment)
    env.reset()

    # Получение имени поведения
    behavior_name = list(env.behavior_specs.keys())[0]
    decision_steps, terminal_steps = env.get_steps(behavior_name)

    try:
        while True:
            # Получение наблюдений для всех активных агентов
            for agent_id in decision_steps.agent_id:
                obs = decision_steps[agent_id].obs[0]  # Предполагается, что наблюдение одно
                action, _ = model.predict(obs, deterministic=True)
                env.set_action_for_agent(behavior_name, agent_id, action)

            # Шаг симуляции
            env.step()
            decision_steps, terminal_steps = env.get_steps(behavior_name)

    except KeyboardInterrupt:
        env.close()

if __name__ == '__main__':
    connect_unity(path_model='models/CarrierRobot v58 5x5 180m.zip')
