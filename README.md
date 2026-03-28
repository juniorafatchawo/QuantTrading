# 📈 QuantTrading Portfolio : Real-Time Options Pricing Engine

[![Build & Test](https://github.com/juniorafatchawo/QuantTrading/actions/workflows/build.yml/badge.svg)](https://github.com/juniorafatchawo/QuantTrading/actions/workflows/build.yml)
![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)
![WPF](https://img.shields.io/badge/UI-WPF-blue)
![Rx.NET](https://img.shields.io/badge/Reactive-Rx.NET-orange)

## 🎯 Aperçu du Projet
Ce projet est une **simulation de poste de trading (Front-Office)** conçue pour démontrer la gestion de flux de données à haute fréquence et le calcul de risques en temps réel. L'application ingère des flux de marché (Ticks), calcule le prix d'options via le modèle **Black-Scholes** et expose les sensibilités (Greques) via une interface réactive.



## 🏗️ Architecture Technique
L'application suit les principes de la **Clean Architecture** pour garantir une séparation stricte entre la logique métier financière et la présentation :

- **QuantTrading.Core** : Modèles de domaine et interfaces (Zero-dependency).
- **QuantTrading.Engine** : Logique de calcul (Black-Scholes) et services de streaming.
- **QuantTrading.UI** : Interface WPF utilisant le pattern **MVVM** avec le CommunityToolkit.

## ⚡ Optimisations Haute Performance (Low Latency Mindset)
Pour répondre aux exigences des environnements de trading, plusieurs optimisations ont été implémentées :

* **Memory Management** : Utilisation de `readonly record struct` pour les messages de marché afin de minimiser les allocations sur le Heap et réduire la pression sur le Garbage Collector (GC).
* **Reactive Programming (Rx.NET)** : Implémentation de pipelines `IObservable` pour traiter les flux. Utilisation de `.Sample()` et `.ObserveOn()` pour découpler le thread de calcul du thread UI (éviter les freezes).
* **Concurrency** : Utilisation intensive de `Task` et de la programmation asynchrone pour assurer une réactivité maximale.

## 📉 Fonctionnalités Clés
- **Real-Time Ticker** : Simulation de flux de prix avec un mouvement brownien géométrique.
- **Option Pricing** : Calcul dynamique du prix des Calls/Puts européens.
- **Risk Metrics** : Calcul en direct des Greques (Delta, Gamma, Vega, Theta, Rho).
- **Professional UI** : Grille dynamique avec effets visuels sur changement de prix (Price Flashing).

## 🛠️ Installation & Lancement
1. Cloner le repo
2. Ouvrir `QuantTrading.sln` dans Visual Studio 2022.
3. Restaurer les packages NuGet.
4. Lancer le projet `QuantTrading.UI`.