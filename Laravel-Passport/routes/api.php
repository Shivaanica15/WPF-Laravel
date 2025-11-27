<?php

use App\Http\Controllers\Api\PostController;
use App\Http\Controllers\AuthController;
use Illuminate\Support\Facades\Route;

Route::post('register', [AuthController::class, 'register'])->name('api.auth.register');
Route::post('login', [AuthController::class, 'login'])->name('api.auth.login');
Route::post('token/refresh', [AuthController::class, 'refresh'])->name('api.auth.refresh');

Route::middleware('auth:api')->group(function (): void {
    Route::get('me', [AuthController::class, 'me'])->name('api.auth.me');
    Route::post('logout', [AuthController::class, 'logout'])->name('api.auth.logout');

    Route::middleware('scope:view-posts')->group(function (): void {
        Route::get('posts', [PostController::class, 'index'])->name('api.posts.index');
        Route::get('posts/{post}', [PostController::class, 'show'])->name('api.posts.show');
    });

    Route::middleware('scopes:manage-posts')->group(function (): void {
        Route::post('posts', [PostController::class, 'store'])->name('api.posts.store');
        Route::put('posts/{post}', [PostController::class, 'update'])->name('api.posts.update');
        Route::delete('posts/{post}', [PostController::class, 'destroy'])->name('api.posts.destroy');
    });
});

