from gymnasium.envs.registration import register

register(
    id='CarrierRobot-v0',
    entry_point='environment.carrier_robot_gym:CarrierRobotEnv',
    kwargs={"width": 10, "height": 10, "render_mode": "human"}
)
