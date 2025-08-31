# FrameworkLog

🔹 One of the biggest challenges in projects has always been **logging complexity**.
Normally, you’d have to install multiple packages (for files, databases, ELK, metadata, etc.) and spend hours configuring them.

I built this library to make it **super simple and all-in-one**.
📦 Just install the package → add some simple config → done!

✨ **Key Features:**

* ⚡ **Unified & Simple Configuration** – no need for multiple packages.
* 📝 **Per-Level Configurations** – customize logging for each level (Info, Error, Warning, etc.).
* 🌍 **Multi-Sink Support** – log to multiple destinations at the same time (File, Database, ELK, etc.).
* 🧩 **Correlation ID Support** – trace requests end-to-end across distributed systems.
* 🎨 **Custom Output Templates** – design your log format exactly how you want, with structured fields & metadata.
* 🏷 **Tag-Based Control** – assign tags to logs and easily enable/disable them or set per-tag minimum levels.
* 🎛 **Log Control Policies** – fine-grained filtering without touching the source code.
* 🚀 **Performance Optimized** – async logging, batching, rotation & compression built-in.
* 📂 **Archiving & Retention** – automatic rotation, archiving, and retention policies.
* 🔌 **Extensible Design** – easily add new sinks (Kafka, Sentry, Console, etc.).
* 🔍 **Advanced Request & Exception Logging** – capture headers, bodies, stack traces, and inner exceptions.
* 🌐 **Global Metadata** – always include app name, version, environment, and global tags.

🎯 **The Goal:**
I designed this package so developers don’t have to waste time juggling multiple logging libraries and complex setups.
With **one package and simple configs**, you get a complete, flexible, and production-ready logging system.

