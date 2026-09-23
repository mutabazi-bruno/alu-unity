# unity_ar_business_card

An augmented reality business card built with Unity and Vuforia. Pointing a device at the printed marker shows an interactive card with my name, job title and links.

## Setup

- Unity 6000.4.7f1
- Vuforia Engine 11.4.4 is **not** included in the repo (the package is over GitHub's 100 MB limit). Download `add-vuforia-package-11-4-4.unitypackage` from [developer.vuforia.com](https://developer.vuforia.com/downloads/sdk) and import it via **Assets > Import Package > Custom Package** before opening the scene.

## Tasks

- **0. Let's see Paul Allen's card**: static business card layout in the `ARBusinessCard` scene (name, job title, email, GitHub, LinkedIn, X, Facebook). Screenshot: [0-layout](0-layout)
- **1. Target acquired**: Vuforia image target database (`BusinessCard`) with the card anchored to the marker; the card hides when the marker is lost.
- **2. Animated intro**: the card elements animate in when the marker is detected.
- **3. Interactive links**: tapping the email and social buttons opens the matching link.
- **4. Build files**: Android (`.apk`) and iOS (Xcode project) builds, zipped as `unity-ar_business_card-Android.zip` and `unity-ar_business_card-iOS.zip`.

## Builds

The build zips are too large for GitHub, so they are on Google Drive: [unity_ar_business_card builds](https://drive.google.com/drive/folders/1bqa4CFjxcKgwmW45qh4ccXZx6kxS8TLH)

## Credits

- Seahorse image marker provided by the Holberton School curriculum, accessed through the ALU intranet
