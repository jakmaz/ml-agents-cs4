# Soccer Agent Training Guide

This guide provides a step-by-step process for training the soccer agent with various features. Each feature is tested and trained individually. Follow the instructions carefully to ensure a smooth training process.

## Training Setup

### 1. Navigate to the Experiment Directory

First, navigate to the `Experiments` directory where the configuration files and training scripts are located. Use the following command to change directories:

```bash
cd /path/to/ml-agents-cs4/Experiments

```

### 2. Training Command

To start the training process, run the following command. This will use the `config.yaml` file located in the `Experiments` directory. Be sure to replace `<experiment_name>` with a unique name for the experiment.

```bash
mlagents-learn config.yaml --run-id=<experiment_name>
```

**Example**:

```bash
mlagents-learn config.yaml --run-id=avoid-fouls-experiment
```

### 3. Start Training

Once the training command is executed, the model will start training according to the configurations set in the `config.yaml` file.

### 4. Monitoring the Training Process

You can monitor the progress of the training in two ways:

- **Unity Editor**: View the training process in real-time.
- **Logs**: Inspect the training logs generated during the process.

If you need to stop the training, press `Ctrl+C` in the terminal.

---

## Experiment Naming Convention

For consistency, it is helpful to follow a specific naming system for experiments. The naming convention works as follows:

- **nPxx**: Represents the number of pitches used in the environment.
- **xxB/D/F/Vx**: Indicates the model’s configuration.
  - `B`: No **B**ackrays
  - `D`: **D**ecoupled movement/vision
  - `F`: **F**air Soccer (i.e., fouls are punished)
  - `S`: **S**ound Sensor
  - `V`: **V**anilla (Base model with no changes)
- **xxxE**: Indicates the environmental variable in use.

### Example: `3PFSE`

This folder name means the model is trained with:

- **3 pitches** (`3P`)
- **Fair Soccer** (fouls punished) + **Sound Sensor** (`FS`)
- An **Environmental Variable** (`E`)

---

## Environment Variables

Currently, the code supports only one environmental variable, `ball_touch`, which can be configured in the `yaml` file. The value for `ball_touch` is set to `0.02` by default, though this is an arbitrary value.

If you're interested in understanding more about the environmental variables or wish to modify this setting, please refer to the relevant configuration files in the `Experiments` directory.

