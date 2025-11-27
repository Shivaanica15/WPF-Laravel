<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Collection;
use Illuminate\Support\Facades\Cache;
use Symfony\Component\HttpFoundation\Response;

class PostController extends Controller
{
    private const CACHE_KEY = 'demo_api_posts';

    public function index(): JsonResponse
    {
        return response()->json(['data' => $this->posts()->values()]);
    }

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'title' => ['required', 'string', 'max:255'],
            'body' => ['required', 'string'],
        ]);

        $posts = $this->posts();
        $nextId = ((int) $posts->max('id')) + 1;

        $post = array_merge($data, [
            'id' => $nextId,
            'user_id' => $request->user()->id,
        ]);

        $posts->push($post);
        $this->persist($posts);

        return response()->json(['data' => $post], Response::HTTP_CREATED);
    }

    public function show(int $post): JsonResponse
    {
        $record = $this->posts()->firstWhere('id', $post);

        abort_if(! $record, Response::HTTP_NOT_FOUND, 'Post not found');

        return response()->json(['data' => $record]);
    }

    public function update(Request $request, int $post): JsonResponse
    {
        $data = $request->validate([
            'title' => ['sometimes', 'string', 'max:255'],
            'body' => ['sometimes', 'string'],
        ]);

        $posts = $this->posts();
        $index = $posts->search(fn ($item) => $item['id'] === $post);

        abort_if($index === false, Response::HTTP_NOT_FOUND, 'Post not found');

        $updated = array_merge($posts[$index], $data, [
            'user_id' => $request->user()->id,
        ]);

        $posts->put($index, $updated);
        $this->persist($posts);

        return response()->json(['data' => $updated]);
    }

    public function destroy(int $post): JsonResponse
    {
        $posts = $this->posts();
        $index = $posts->search(fn ($item) => $item['id'] === $post);

        abort_if($index === false, Response::HTTP_NOT_FOUND, 'Post not found');

        $deleted = $posts->pull($index);
        $this->persist($posts->values());

        return response()->json(['data' => $deleted]);
    }

    private function posts(): Collection
    {
        return collect(Cache::get(self::CACHE_KEY, $this->seedPosts()));
    }

    private function persist(Collection $posts): void
    {
        Cache::forever(self::CACHE_KEY, $posts->values()->all());
    }

    private function seedPosts(): array
    {
        return [
            ['id' => 1, 'title' => 'Getting started with Passport', 'body' => 'Use bearer tokens to access protected routes.', 'user_id' => 1],
            ['id' => 2, 'title' => 'Refresh tokens', 'body' => 'Store refresh tokens securely and rotate them frequently.', 'user_id' => 1],
        ];
    }
}

