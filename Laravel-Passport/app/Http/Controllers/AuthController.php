<?php

namespace App\Http\Controllers;

use App\Models\User;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Hash;
use Illuminate\Validation\ValidationException;
use Laravel\Passport\Passport;
use Symfony\Component\HttpFoundation\Response;

class AuthController extends Controller
{

    public function register(Request $request): JsonResponse
    {
        $data = $request->validate([
            'name' => ['required', 'string', 'max:255'],
            'email' => ['required', 'email', 'unique:users,email'],
            'password' => ['required', 'confirmed', 'min:8'],
            'scope' => ['nullable', 'string'],
        ]);

        $user = User::create([
            'name' => $data['name'],
            'email' => $data['email'],
            'password' => Hash::make($data['password']),
        ]);

        $tokens = $this->issuePasswordToken(
            username: $user->email,
            password: $data['password'],
            scope: $data['scope'] ?? null,
        );

        return response()->json([
            'user' => $user,
            'tokens' => $tokens,
        ], Response::HTTP_CREATED);
    }

    public function login(Request $request): JsonResponse
    {
        $credentials = $request->validate([
            'email' => ['required', 'email'],
            'password' => ['required', 'string'],
            'scope' => ['nullable', 'string'],
        ]);

        $user = User::where('email', $credentials['email'])->first();

        if (! $user || ! Hash::check($credentials['password'], $user->password)) {
            throw ValidationException::withMessages([
                'email' => ['The provided credentials are invalid.'],
            ]);
        }

        $tokens = $this->issuePasswordToken(
            username: $credentials['email'],
            password: $credentials['password'],
            scope: $credentials['scope'] ?? null,
        );

        return response()->json([
            'user' => $user,
            'tokens' => $tokens,
        ]);
    }

    public function refresh(Request $request): JsonResponse
    {
        $data = $request->validate([
            'refresh_token' => ['required', 'string'],
            'scope' => ['nullable', 'string'],
        ]);

        $tokens = $this->proxyTokenRequest([
            'grant_type' => 'refresh_token',
            'refresh_token' => $data['refresh_token'],
            'scope' => $data['scope'] ?? 'view-posts',
        ]);

        return response()->json(['tokens' => $tokens]);
    }

    public function logout(Request $request): JsonResponse
    {
        $token = $request->user()?->token();

        if ($token) {
            $token->revoke();

            Passport::refreshToken()
                ->newQuery()
                ->where('access_token_id', $token->id)
                ->update(['revoked' => true]);
        }

        return response()->json(['message' => 'Logged out']);
    }

    public function me(Request $request): JsonResponse
    {
        return response()->json(['user' => $request->user()]);
    }

    private function issuePasswordToken(string $username, string $password, ?string $scope = null): array
    {
        return $this->proxyTokenRequest([
            'grant_type' => 'password',
            'username' => $username,
            'password' => $password,
            'scope' => $scope ?? 'view-posts',
        ]);
    }

    private function proxyTokenRequest(array $data): array
    {
        $client = $this->passwordClient();

        $payload = array_merge($data, [
            'client_id' => $client['id'],
            'client_secret' => $client['secret'],
        ]);

        $tokenRequest = \Illuminate\Http\Request::create('/oauth/token', 'POST', $payload);
        $response = app()->handle($tokenRequest);

        if ($response->getStatusCode() >= Response::HTTP_BAD_REQUEST) {
            throw ValidationException::withMessages([
                'token' => ['Unable to issue Passport token.'],
            ]);
        }

        /** @var array<string, mixed> $decoded */
        $decoded = json_decode($response->getContent(), true);

        return $decoded;
    }

    /**
     * @return array{id: string, secret: string}
     */
    private function passwordClient(): array
    {
        $clientId = config('services.passport.password_client_id');
        $clientSecret = config('services.passport.password_client_secret');

        if ($clientId && $clientSecret) {
            return ['id' => $clientId, 'secret' => $clientSecret];
        }

        throw ValidationException::withMessages([
            'client' => ['Set PASSPORT_PASSWORD_CLIENT_ID and PASSPORT_PASSWORD_CLIENT_SECRET in your .env file.'],
        ]);
    }
}

