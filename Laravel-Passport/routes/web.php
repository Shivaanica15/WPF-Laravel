<?php

use App\Http\Controllers\ProfileController;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Route;
use Laravel\Passport\ClientRepository;

Route::get('/', function () {
    return view('welcome');
});

Route::get('/dashboard', function () {
    return view('dashboard');
})->middleware(['auth', 'verified'])->name('dashboard');

Route::middleware('auth')->group(function () {

    // Profile Routes
    Route::get('/profile', [ProfileController::class, 'edit'])->name('profile.edit');
    Route::patch('/profile', [ProfileController::class, 'update'])->name('profile.update');
    Route::delete('/profile', [ProfileController::class, 'destroy'])->name('profile.destroy');

    /**
     * =====================================
     *   CLIENTS PAGE + CLIENT CREATION
     * =====================================
     */

    // Show Clients Page
    Route::get('/dashboard/clients', function (Request $request) {
        return view('clients', [
            'clients' => $request->user()->clients,
        ]);
    })->name('dashboard.clients');

    // Create new OAuth client
    Route::post('/dashboard/clients/create', function (Request $request, ClientRepository $clients) {
        $data = $request->validate([
            'name' => 'required|string|max:255',
            'redirect' => 'required|url',
        ]);

        $client = $clients->createAuthorizationCodeGrantClient(
            name: $data['name'],
            redirectUris: [$data['redirect']],
            confidential: true,
            user: $request->user()
        );

        return back()->with('created_client', [
            'id' => $client->id,
            'secret' => $client->plainSecret,
        ]);
    })->name('dashboard.clients.create');
});

require __DIR__ . '/auth.php';
